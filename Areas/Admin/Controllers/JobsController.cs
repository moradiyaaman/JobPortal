using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public JobsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var jobs = await _context.Jobs
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();
            return View(jobs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Provider)
                .Include(j => j.Applications)
                .ThenInclude(a => a.Applicant)
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        public IActionResult Create()
        {
            return View(new Job());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Job job, IFormFile companyLogo)
        {
            if (companyLogo != null && companyLogo.Length > 0 && !IsLogoExtensionAllowed(companyLogo))
            {
                ModelState.AddModelError(string.Empty, "Please upload a logo in PNG, JPG, or SVG format.");
                return View(job);
            }

            if (!ModelState.IsValid)
            {
                return View(job);
            }

            if (companyLogo != null && companyLogo.Length > 0)
            {
                job.CompanyLogoPath = await SaveLogo(companyLogo);
            }

            job.PostedAt = DateTime.UtcNow;
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Job created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            return View(job);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Job job, IFormFile companyLogo)
        {
            if (id != job.Id)
            {
                return BadRequest();
            }

            var existingJob = await _context.Jobs.FindAsync(id);
            if (existingJob == null)
            {
                return NotFound();
            }

            if (companyLogo != null && companyLogo.Length > 0 && !IsLogoExtensionAllowed(companyLogo))
            {
                ModelState.AddModelError(string.Empty, "Please upload a logo in PNG, JPG, or SVG format.");
                return View(existingJob);
            }

            if (!ModelState.IsValid)
            {
                return View(existingJob);
            }

            existingJob.Title = job.Title;
            existingJob.Description = job.Description;
            existingJob.Qualifications = job.Qualifications;
            existingJob.Experience = job.Experience;
            existingJob.Skills = job.Skills;
            existingJob.CompanyName = job.CompanyName;
            existingJob.CompanyWebsite = job.CompanyWebsite;
            existingJob.Location = job.Location;
            existingJob.Country = job.Country;
            existingJob.JobType = job.JobType;
            existingJob.Salary = job.Salary;
            existingJob.IsActive = job.IsActive;

            if (companyLogo != null && companyLogo.Length > 0)
            {
                existingJob.CompanyLogoPath = await SaveLogo(companyLogo);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Job updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
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
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            job.IsActive = !job.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = job.IsActive ? "Job listing activated." : "Job listing hidden.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Applications(int id)
        {
            var job = await _context.Jobs
                .Include(j => j.Applications)
                .ThenInclude(a => a.Applicant)
                .FirstOrDefaultAsync(j => j.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        private async Task<string> SaveLogo(IFormFile logoFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "logos");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"logo_{Guid.NewGuid()}{Path.GetExtension(logoFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await logoFile.CopyToAsync(stream);
            }
            return $"/uploads/logos/{fileName}";
        }

        private static bool IsLogoExtensionAllowed(IFormFile file)
        {
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }
    }
}
