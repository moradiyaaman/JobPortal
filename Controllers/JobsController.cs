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

            var viewModel = new JobFilterViewModel
            {
                Keyword = filter?.Keyword,
                Country = filter?.Country,
                JobType = filter?.JobType,
                PostedWithinDays = filter?.PostedWithinDays,
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
                ProviderDisplayName = job.ProviderDisplayName,
                ProviderSummary = job.ProviderSummary,
                ProviderWebsite = job.CompanyWebsite
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int id)
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

            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.JobId == job.Id && a.ApplicantId == user.Id);

            if (!alreadyApplied)
            {
                var application = new JobApplication
                {
                    JobId = job.Id,
                    ApplicantId = user.Id
                };
                _context.JobApplications.Add(application);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Application submitted successfully.";
            }
            else
            {
                TempData["Info"] = "You've already applied for this job.";
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
                    Status = a.Status
                })
                .ToListAsync();

            return View(applications);
        }
    }
}
