using System;
using System.Linq;
using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Models;
using JobPortal.Services;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IAtsScorer _atsScorer;
        private static readonly string[] ApplicationStatuses = new[]
        {
            "Applied",
            "Under Review",
            "Interviewing",
            "Shortlisted",
            "Offer",
            "Rejected"
        };

        public JobsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService, IAtsScorer atsScorer)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _atsScorer = atsScorer;
        }

        public async Task<IActionResult> Index()
        {
            var provider = await _userManager.GetUserAsync(User);
            if (provider == null)
            {
                return Challenge();
            }

            var jobs = await _context.Jobs
                .Where(j => j.ProviderId == provider.Id)
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new ProviderJobListItemViewModel
                {
                    Id = j.Id,
                    Title = j.Title,
                    PostedAt = j.PostedAt,
                    IsActive = j.IsActive,
                    ApplicationCount = j.Applications.Count
                })
                .ToListAsync();

            ViewBag.CompanyName = provider.CompanyName ?? provider.FullName;
            return View(jobs);
        }

        public async Task<IActionResult> Create()
        {
            var provider = await _userManager.GetUserAsync(User);
            if (provider == null)
            {
                return Challenge();
            }

            var model = BuildFormModel(new ProviderJobFormViewModel(), provider);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProviderJobFormViewModel model)
        {
            var provider = await _userManager.GetUserAsync(User);
            if (provider == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                model = BuildFormModel(model, provider);
                return View(model);
            }

            var job = new Job
            {
                Title = model.Title?.Trim(),
                Description = model.Description?.Trim(),
                Qualifications = model.Qualifications?.Trim(),
                Experience = model.Experience?.Trim(),
                Skills = model.Skills?.Trim(),
                Salary = model.Salary?.Trim(),
                Location = string.IsNullOrWhiteSpace(model.Location) ? provider.CompanyLocation : model.Location?.Trim(),
                Country = string.IsNullOrWhiteSpace(model.Country) ? provider.Country : model.Country?.Trim(),
                JobType = model.JobType?.Trim(),
                IsActive = model.IsActive,
                PostedAt = DateTime.UtcNow,
                ProviderId = provider.Id,
                CompanyName = provider.CompanyName ?? provider.FullName,
                CompanyWebsite = provider.CompanyWebsite,
                CompanyLogoPath = provider.CompanyLogoPath,
                ProviderDisplayName = provider.CompanyName ?? provider.FullName,
                ProviderSummary = provider.CompanyDescription,
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Job posted successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var provider = await _userManager.GetUserAsync(User);
            if (provider == null)
            {
                return Challenge();
            }

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.ProviderId == provider.Id);
            if (job == null)
            {
                return NotFound();
            }

            var model = BuildFormModel(new ProviderJobFormViewModel
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Qualifications = job.Qualifications,
                Experience = job.Experience,
                Skills = job.Skills,
                Salary = job.Salary,
                Location = job.Location,
                Country = job.Country,
                JobType = job.JobType,
                IsActive = job.IsActive
            }, provider);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProviderJobFormViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var provider = await _userManager.GetUserAsync(User);
            if (provider == null)
            {
                return Challenge();
            }

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.ProviderId == provider.Id);
            if (job == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model = BuildFormModel(model, provider);
                return View(model);
            }

            job.Title = model.Title?.Trim();
            job.Description = model.Description?.Trim();
            job.Qualifications = model.Qualifications?.Trim();
            job.Experience = model.Experience?.Trim();
            job.Skills = model.Skills?.Trim();
            job.Salary = model.Salary?.Trim();
            job.Location = string.IsNullOrWhiteSpace(model.Location) ? provider.CompanyLocation : model.Location?.Trim();
            job.Country = string.IsNullOrWhiteSpace(model.Country) ? provider.Country : model.Country?.Trim();
            job.JobType = model.JobType?.Trim();
            job.IsActive = model.IsActive;
            job.CompanyName = provider.CompanyName ?? provider.FullName;
            job.CompanyWebsite = provider.CompanyWebsite;
            job.CompanyLogoPath = provider.CompanyLogoPath;
            job.ProviderDisplayName = provider.CompanyName ?? provider.FullName;
            job.ProviderSummary = provider.CompanyDescription;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Job updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var provider = await _userManager.GetUserAsync(User);
            if (provider == null)
            {
                return Challenge();
            }

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.ProviderId == provider.Id);
            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Job deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var provider = await _userManager.GetUserAsync(User);
            if (provider == null)
            {
                return Challenge();
            }

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.ProviderId == provider.Id);
            if (job == null)
            {
                return NotFound();
            }

            job.IsActive = !job.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = job.IsActive ? "Job listing activated." : "Job listing paused.";
            return RedirectToAction(nameof(Index));
        }

        private static ProviderJobFormViewModel BuildFormModel(ProviderJobFormViewModel model, ApplicationUser provider)
        {
            model.ProviderCompanyName = provider.CompanyName ?? provider.FullName;
            model.ProviderCompanyWebsite = provider.CompanyWebsite;
            model.ProviderCompanyLocation = provider.CompanyLocation ?? provider.Country;
            model.ProviderCompanyDescription = provider.CompanyDescription;
            return model;
        }

        [Authorize]
        public async Task<IActionResult> Applications(int id, string sort = "rank")
        {
            var provider = await _userManager.GetUserAsync(User);
            if (provider == null)
            {
                return Challenge();
            }

            var job = await _context.Jobs
                .Include(j => j.Applications)
                .ThenInclude(a => a.Applicant)
                .FirstOrDefaultAsync(j => j.Id == id && j.ProviderId == provider.Id);

            if (job == null)
            {
                return NotFound();
            }

            var sortOrder = string.Equals(sort, "date", StringComparison.OrdinalIgnoreCase) ? "date" : "rank";

            // Compute ATS rank for each app (best-effort). Fallback to applied date if scorer fails.
            var ranked = await Task.WhenAll(job.Applications
                .Select(async a => new
                {
                    App = a,
                    Rank = await _atsScorer.RankScoreAsync(a.Applicant, job, a.CoverLetter) ?? new AtsRankResult
                    {
                        Score = 0,
                        MatchedKeywords = Array.Empty<string>(),
                        MissingKeywords = Array.Empty<string>()
                    }
                }));

            var ordered = sortOrder == "date"
                ? ranked.OrderByDescending(x => x.App.AppliedAt).ThenByDescending(x => x.Rank.Score)
                : ranked.OrderByDescending(x => x.Rank.Score).ThenByDescending(x => x.App.AppliedAt);

            var model = new ProviderJobApplicationsViewModel
            {
                JobId = job.Id,
                JobTitle = job.Title,
                SortOrder = sortOrder,
                HasAtsData = ranked.Any(x => x.Rank.Score > 0 || (x.Rank.MatchedKeywords?.Length ?? 0) > 0),
                Applications = ordered
                    .Select(x => new ProviderApplicationItemViewModel
                    {
                        ApplicationId = x.App.Id,
                        ApplicantName = x.App.Applicant.FullName ?? x.App.Applicant.Email,
                        ApplicantEmail = x.App.Applicant.Email,
                        AppliedAt = x.App.AppliedAt,
                        Status = x.App.Status,
                        ResumeUrl = x.App.Applicant.ResumeFileName,
                        CoverLetter = x.App.CoverLetter,
                        AtsScore = x.Rank.Score,
                        AtsMatchedKeywords = x.Rank.MatchedKeywords ?? Array.Empty<string>(),
                        AtsMissingKeywords = x.Rank.MissingKeywords ?? Array.Empty<string>()
                    })
            };

            ViewBag.StatusOptions = ApplicationStatuses;
            ViewBag.RankedByAts = sortOrder != "date";
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(int id, string status, string sort)
        {
            var provider = await _userManager.GetUserAsync(User);
            if (provider == null)
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(status) || !ApplicationStatuses.Contains(status))
            {
                TempData["Error"] = "Select a valid status before saving.";
                return RedirectToAction(nameof(Index));
            }

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Applicant)
                .FirstOrDefaultAsync(a => a.Id == id && a.Job.ProviderId == provider.Id);

            if (application == null)
            {
                return NotFound();
            }

            application.Status = status;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(application.Applicant?.Email))
            {
                var subject = $"Update on your application for {application.Job.Title}";
                var applicantName = application.Applicant.FullName ?? application.Applicant.Email;
                var body = $"<p>Hi {applicantName},</p>" +
                           $"<p>Your application for <strong>{application.Job.Title}</strong> has been marked as <strong>{status}</strong>.</p>" +
                           "<p>We'll keep you posted on any further updates.</p>" +
                           "<p>— The hiring team</p>";

                try
                {
                    await _emailService.SendAsync(application.Applicant.Email, subject, body);
                }
                catch (Exception)
                {
                    // If email fails we still consider the status update successful.
                }
            }

            TempData["Success"] = "Application status updated.";
            var redirectSort = string.Equals(sort, "date", StringComparison.OrdinalIgnoreCase) ? "date" : "rank";
            return RedirectToAction(nameof(Applications), new { id = application.JobId, sort = redirectSort });
        }
    }
}
