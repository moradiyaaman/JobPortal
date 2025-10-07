using System.Linq;
using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Areas.Provider.Controllers
{
    [Area("Provider")]
    [Authorize(Roles = "Provider")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var jobsQuery = _context.Jobs.Where(j => j.ProviderId == user.Id);

            var model = new ProviderDashboardViewModel
            {
                CompanyName = user.CompanyName ?? user.FullName,
                CompanyLocation = user.CompanyLocation ?? user.Country,
                CompanyWebsite = user.CompanyWebsite,
                CompanyDescription = user.CompanyDescription,
                TotalJobs = await jobsQuery.CountAsync(),
                ActiveJobs = await jobsQuery.CountAsync(j => j.IsActive),
                TotalApplications = await _context.JobApplications.CountAsync(a => a.Job.ProviderId == user.Id),
                RecentJobs = await jobsQuery
                    .OrderByDescending(j => j.PostedAt)
                    .Take(5)
                    .Select(j => new ProviderJobListItemViewModel
                    {
                        Id = j.Id,
                        Title = j.Title,
                        PostedAt = j.PostedAt,
                        IsActive = j.IsActive,
                        ApplicationCount = j.Applications.Count
                    })
                    .ToListAsync(),
                RecentApplications = await _context.JobApplications
                    .Where(a => a.Job.ProviderId == user.Id)
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(5)
                    .Select(a => new ProviderRecentApplicationViewModel
                    {
                        JobId = a.JobId,
                        JobTitle = a.Job.Title,
                        ApplicantName = a.Applicant.FullName ?? a.Applicant.Email,
                        AppliedAt = a.AppliedAt,
                        Status = a.Status
                    })
                    .ToListAsync()
            };

            return View(model);
        }
    }
}
