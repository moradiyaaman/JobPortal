using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] JobFilterViewModel filter)
        {
            var jobsQuery = _context.Jobs
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter?.Keyword))
            {
                var keyword = filter.Keyword.Trim();
                jobsQuery = jobsQuery.Where(j =>
                    j.Title.Contains(keyword) ||
                    j.CompanyName.Contains(keyword) ||
                    j.Location.Contains(keyword) ||
                    j.Description.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(filter?.Country))
            {
                jobsQuery = jobsQuery.Where(j => j.Country == filter.Country);
            }

            if (!string.IsNullOrWhiteSpace(filter?.JobType))
            {
                jobsQuery = jobsQuery.Where(j => j.JobType == filter.JobType);
            }

            if (filter?.PostedWithinDays != null)
            {
                var minDate = DateTime.UtcNow.AddDays(-filter.PostedWithinDays.Value);
                jobsQuery = jobsQuery.Where(j => j.PostedAt >= minDate);
            }

            var jobs = await jobsQuery.Take(200).ToListAsync();

            var currentUserId = User.Identity?.IsAuthenticated == true
                ? _userManager.GetUserId(User)
                : null;

            var appliedJobIds = currentUserId == null
                ? new HashSet<int>()
                : (await _context.JobApplications
                    .Where(a => a.ApplicantId == currentUserId)
                    .Select(a => a.JobId)
                    .ToListAsync()).ToHashSet();

            var savedJobIds = currentUserId == null
                ? new HashSet<int>()
                : (await _context.SavedJobs
                    .Where(s => s.UserId == currentUserId)
                    .Select(s => s.JobId)
                    .ToListAsync()).ToHashSet();

            var viewModel = new JobFilterViewModel
            {
                Keyword = filter?.Keyword,
                Country = filter?.Country,
                JobType = filter?.JobType,
                PostedWithinDays = filter?.PostedWithinDays,
                CanManageSaved = currentUserId != null,
                AvailableCountries = await _context.Jobs
                    .Where(j => j.IsActive && !string.IsNullOrEmpty(j.Country))
                    .Select(j => j.Country)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync(),
                AvailableJobTypes = await _context.Jobs
                    .Where(j => j.IsActive && !string.IsNullOrEmpty(j.JobType))
                    .Select(j => j.JobType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync(),
                Jobs = jobs.Select(j => new JobListItemViewModel
                {
                    Id = j.Id,
                    Title = j.Title,
                    CompanyName = j.CompanyName,
                    CompanyLogoPath = j.CompanyLogoPath,
                    CompanyWebsite = j.CompanyWebsite,
                    Location = j.Location,
                    Country = j.Country,
                    JobType = j.JobType,
                    Salary = j.Salary,
                    PostedAt = j.PostedAt,
                    IsApplied = appliedJobIds.Contains(j.Id),
                    IsSaved = savedJobIds.Contains(j.Id),
                    ProviderDisplayName = j.ProviderDisplayName,
                    ProviderSummary = j.ProviderSummary
                })
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.IsActive);
            if (job == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isApplied = userId != null && await _context.JobApplications
                .AnyAsync(a => a.JobId == job.Id && a.ApplicantId == userId);

            var canApply = User.Identity?.IsAuthenticated == true;
            var canSave = canApply;
            var isSaved = canSave && userId != null && await _context.SavedJobs
                .AnyAsync(s => s.JobId == job.Id && s.UserId == userId);

            var viewModel = new JobDetailViewModel
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Qualifications = job.Qualifications,
                Experience = job.Experience,
                Skills = job.Skills,
                Salary = job.Salary,
                CompanyName = job.CompanyName,
                CompanyWebsite = job.CompanyWebsite,
                CompanyLogoPath = job.CompanyLogoPath,
                Location = job.Location,
                Country = job.Country,
                JobType = job.JobType,
                PostedAt = job.PostedAt,
                IsApplied = isApplied,
                CanApply = canApply,
                CanSave = canSave,
                IsSaved = isSaved,
                ProviderDisplayName = job.ProviderDisplayName,
                ProviderSummary = job.ProviderSummary,
                ProviderWebsite = job.CompanyWebsite
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int id, string coverLetter)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.IsActive);
            if (job == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(user.ResumeFileName))
            {
                TempData["Error"] = "Please upload your resume in your profile before applying.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var trimmedCoverLetter = string.IsNullOrWhiteSpace(coverLetter) ? null : coverLetter.Trim();
            if (trimmedCoverLetter != null && trimmedCoverLetter.Length > 4000)
            {
                TempData["Error"] = "Cover letter is too long. Please keep it under 4000 characters.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var application = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.JobId == job.Id && a.ApplicantId == user.Id);

            if (application == null)
            {
                application = new JobApplication
                {
                    JobId = job.Id,
                    ApplicantId = user.Id,
                    CoverLetter = trimmedCoverLetter
                };
                _context.JobApplications.Add(application);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Application submitted successfully.";
            }
            else
            {
                application.CoverLetter = trimmedCoverLetter;
                await _context.SaveChangesAsync();
                TempData["Info"] = "Your existing application has been updated.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyApplications()
        {
            var userId = _userManager.GetUserId(User);
            var applications = await _context.JobApplications
                .Where(a => a.ApplicantId == userId)
                .OrderByDescending(a => a.AppliedAt)
                .Select(a => new MyApplicationViewModel
                {
                    JobId = a.JobId,
                    JobTitle = a.Job.Title,
                    CompanyName = a.Job.CompanyName,
                    Location = a.Job.Location,
                    AppliedAt = a.AppliedAt,
                    Status = a.Status,
                    CoverLetter = a.CoverLetter
                })
                .ToListAsync();

            return View(applications);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Saved()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var appliedJobIds = await _context.JobApplications
                .Where(a => a.ApplicantId == userId)
                .Select(a => a.JobId)
                .ToListAsync();

            var savedJobs = await _context.SavedJobs
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SavedAt)
                .Select(s => new JobListItemViewModel
                {
                    Id = s.JobId,
                    Title = s.Job.Title,
                    CompanyName = s.Job.CompanyName,
                    CompanyLogoPath = s.Job.CompanyLogoPath,
                    CompanyWebsite = s.Job.CompanyWebsite,
                    Location = s.Job.Location,
                    Country = s.Job.Country,
                    JobType = s.Job.JobType,
                    Salary = s.Job.Salary,
                    PostedAt = s.Job.PostedAt,
                    IsApplied = appliedJobIds.Contains(s.JobId),
                    IsSaved = true,
                    ProviderDisplayName = s.Job.ProviderDisplayName,
                    ProviderSummary = s.Job.ProviderSummary
                })
                .ToListAsync();

            return View(savedJobs);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int id, string returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.IsActive);
            if (job == null)
            {
                return NotFound();
            }

            var alreadySaved = await _context.SavedJobs.AnyAsync(s => s.JobId == id && s.UserId == userId);
            if (!alreadySaved)
            {
                _context.SavedJobs.Add(new SavedJob
                {
                    JobId = id,
                    UserId = userId
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Job saved to your list.";
            }

            return RedirectToLocal(returnUrl, nameof(Details), new { id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsave(int id, string returnUrl = null)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var savedJob = await _context.SavedJobs.FirstOrDefaultAsync(s => s.JobId == id && s.UserId == userId);
            if (savedJob != null)
            {
                _context.SavedJobs.Remove(savedJob);
                await _context.SaveChangesAsync();
                TempData["Info"] = "Job removed from your saved list.";
            }

            return RedirectToLocal(returnUrl, nameof(Details), new { id });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Alerts()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var alerts = await _context.JobAlertSubscriptions
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new JobAlertSubscriptionViewModel
                {
                    Id = a.Id,
                    Keyword = a.Keyword,
                    Country = a.Country,
                    JobType = a.JobType,
                    CreatedAt = a.CreatedAt,
                    LastNotifiedAt = a.LastNotifiedAt
                })
                .ToListAsync();

            foreach (var alert in alerts)
            {
                alert.MatchesSinceLastNotification = await CountMatchesForAlertAsync(alert);
            }

            var model = new ManageJobAlertsViewModel
            {
                Alerts = alerts,
                AvailableCountries = await _context.Jobs
                    .Where(j => j.IsActive && !string.IsNullOrEmpty(j.Country))
                    .Select(j => j.Country)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync(),
                AvailableJobTypes = await _context.Jobs
                    .Where(j => j.IsActive && !string.IsNullOrEmpty(j.JobType))
                    .Select(j => j.JobType)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync()
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAlert(JobAlertFormModel model)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var keyword = string.IsNullOrWhiteSpace(model.Keyword) ? null : model.Keyword.Trim();
            var country = string.IsNullOrWhiteSpace(model.Country) ? null : model.Country.Trim();
            var jobType = string.IsNullOrWhiteSpace(model.JobType) ? null : model.JobType.Trim();

            if (keyword == null && country == null && jobType == null)
            {
                TempData["Error"] = "Add at least one filter (keyword, country, or job type) to create an alert.";
                return RedirectToAction(nameof(Alerts));
            }

            var exists = await _context.JobAlertSubscriptions.AnyAsync(a =>
                a.UserId == userId &&
                a.Keyword == keyword &&
                a.Country == country &&
                a.JobType == jobType);

            if (exists)
            {
                TempData["Info"] = "You already have an alert with the same filters.";
                return RedirectToAction(nameof(Alerts));
            }

            _context.JobAlertSubscriptions.Add(new JobAlertSubscription
            {
                UserId = userId,
                Keyword = keyword,
                Country = country,
                JobType = jobType
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Job alert created.";
            return RedirectToAction(nameof(Alerts));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var alert = await _context.JobAlertSubscriptions.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (alert != null)
            {
                _context.JobAlertSubscriptions.Remove(alert);
                await _context.SaveChangesAsync();
                TempData["Info"] = "Job alert removed.";
            }

            return RedirectToAction(nameof(Alerts));
        }

        private IActionResult RedirectToLocal(string returnUrl, string fallbackAction, object routeValues)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(fallbackAction, routeValues);
        }

        private async Task<int> CountMatchesForAlertAsync(JobAlertSubscriptionViewModel alert)
        {
            var query = _context.Jobs.AsQueryable().Where(j => j.IsActive);

            if (!string.IsNullOrWhiteSpace(alert.Keyword))
            {
                var keyword = alert.Keyword.Trim();
                query = query.Where(j =>
                    j.Title.Contains(keyword) ||
                    j.Description.Contains(keyword) ||
                    j.Skills.Contains(keyword) ||
                    j.CompanyName.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(alert.Country))
            {
                query = query.Where(j => j.Country == alert.Country);
            }

            if (!string.IsNullOrWhiteSpace(alert.JobType))
            {
                query = query.Where(j => j.JobType == alert.JobType);
            }

            var since = alert.LastNotifiedAt ?? DateTime.UtcNow.AddDays(-7);
            query = query.Where(j => j.PostedAt >= since);

            return await query.CountAsync();
        }
    }
}
