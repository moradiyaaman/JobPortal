using System;
using System.Linq;
using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Services;
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
        private static readonly string[] ApplicationStatuses = new[]
        {
            "Applied",
            "Under Review",
            "Interviewing",
            "Shortlisted",
            "Offer",
            "Rejected"
        };

        public ApplicationsController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var applications = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Applicant)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
            ViewBag.StatusOptions = ApplicationStatuses;
            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
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
            return RedirectToAction(nameof(Index));
        }
    }
}
