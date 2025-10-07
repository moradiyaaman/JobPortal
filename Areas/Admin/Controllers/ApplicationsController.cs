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
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applications = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Applicant)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
            return View(applications);
        }
    }
}
