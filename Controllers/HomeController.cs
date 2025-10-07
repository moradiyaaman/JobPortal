using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var jobs = await _context.Jobs
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedAt)
                .Take(6)
                .Select(j => new JobListItemViewModel
                {
                    Id = j.Id,
                    Title = j.Title,
                    CompanyName = j.CompanyName,
                    CompanyLogoPath = j.CompanyLogoPath,
                    Location = j.Location,
                    JobType = j.JobType,
                    Salary = j.Salary,
                    PostedAt = j.PostedAt
                })
                .ToListAsync();

            var stats = new HomeStatsViewModel
            {
                TotalJobs = await _context.Jobs.CountAsync(j => j.IsActive),
                TotalApplications = await _context.JobApplications.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                RecentJobs = jobs
            };

            return View(stats);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
