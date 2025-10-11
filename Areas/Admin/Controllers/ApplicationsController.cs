using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Services;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ApplicationsController : Controller
    {
    private readonly ApplicationDbContext _context;
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

        public ApplicationsController(ApplicationDbContext context, IEmailService emailService, IAtsScorer atsScorer)
        {
            _context = context;
            _emailService = emailService;
            _atsScorer = atsScorer;
        }

        public async Task<IActionResult> Index(string sort = "date")
        {
            var applications = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Applicant)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
            var sortOrder = string.Equals(sort, "rank", StringComparison.OrdinalIgnoreCase) ? "rank" : "date";

            var tasks = applications.Select(async app =>
            {
                var rank = await _atsScorer.RankScoreAsync(app.Applicant, app.Job, app.CoverLetter) ?? new AtsRankResult
                {
                    Score = 0,
                    MatchedKeywords = Array.Empty<string>(),
                    MissingKeywords = Array.Empty<string>()
                };

                return new AdminApplicationListItemViewModel
                {
                    ApplicationId = app.Id,
                    JobId = app.JobId,
                    JobTitle = app.Job?.Title,
                    ApplicantName = app.Applicant?.FullName ?? app.Applicant?.Email,
                    ApplicantEmail = app.Applicant?.Email,
                    AppliedAt = app.AppliedAt,
                    Status = app.Status,
                    ResumeUrl = app.Applicant?.ResumeFileName,
                    CoverLetter = app.CoverLetter,
                    AtsScore = rank.Score,
                    AtsMatchedKeywords = rank.MatchedKeywords ?? Array.Empty<string>(),
                    AtsMissingKeywords = rank.MissingKeywords ?? Array.Empty<string>()
                };
            });

            var withRank = await Task.WhenAll(tasks);

            IEnumerable<AdminApplicationListItemViewModel> ordered = sortOrder == "rank"
                ? withRank.OrderByDescending(a => a.AtsScore ?? 0).ThenByDescending(a => a.AppliedAt)
                : withRank.OrderByDescending(a => a.AppliedAt).ThenByDescending(a => a.AtsScore ?? 0);

            var model = new AdminApplicationsIndexViewModel
            {
                SortOrder = sortOrder,
                HasAtsData = withRank.Any(a => (a.AtsScore ?? 0) > 0 || (a.AtsMatchedKeywords?.Count ?? 0) > 0),
                Applications = ordered.ToList()
            };

            ViewBag.StatusOptions = ApplicationStatuses;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string sort)
        {
            if (string.IsNullOrWhiteSpace(status) || !ApplicationStatuses.Contains(status))
            {
                TempData["Error"] = "Select a valid status.";
                return RedirectToAction(nameof(Index));
            }

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Applicant)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            application.Status = status;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(application.Applicant?.Email))
            {
                var applicantName = application.Applicant.FullName ?? application.Applicant.Email;
                var subject = $"Update on your application for {application.Job.Title}";
                var body = $"<p>Hi {applicantName},</p>" +
                           $"<p>Your application for <strong>{application.Job.Title}</strong> is now marked as <strong>{status}</strong>.</p>" +
                           "<p>You can sign in to check further details anytime.</p>" +
                           "<p>— JobPortal team</p>";

                try
                {
                    await _emailService.SendAsync(application.Applicant.Email, subject, body);
                }
                catch (Exception)
                {
                    // ignore email failures for now
                }
            }

            TempData["Success"] = "Application status updated.";
            var redirectSort = string.Equals(sort, "rank", StringComparison.OrdinalIgnoreCase) ? "rank" : "date";
            return RedirectToAction(nameof(Index), new { sort = redirectSort });
        }
    }
}
