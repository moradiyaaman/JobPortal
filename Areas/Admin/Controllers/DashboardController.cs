using System.Linq;
using System.Threading.Tasks;
using JobPortal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalJobs = await _context.Jobs.CountAsync(),
                ActiveJobs = await _context.Jobs.CountAsync(j => j.IsActive),
                TotalApplications = await _context.JobApplications.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                ContactMessagesCount = await _context.ContactMessages.CountAsync(),
                RecentApplications = await _context.JobApplications
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(5)
                    .Select(a => new AdminRecentApplication
                    {
                        JobTitle = a.Job.Title,
                        ApplicantName = a.Applicant.FullName,
                        AppliedAt = a.AppliedAt,
                        Status = a.Status
                    })
                    .ToListAsync()
            };

            return View(model);
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalApplications { get; set; }
        public int TotalUsers { get; set; }
        public int ContactMessagesCount { get; set; }
        public System.Collections.Generic.IEnumerable<AdminRecentApplication> RecentApplications { get; set; }
    }

    public class AdminRecentApplication
    {
        public string JobTitle { get; set; }
        public string ApplicantName { get; set; }
        public System.DateTime AppliedAt { get; set; }
        public string Status { get; set; }
    }
}
