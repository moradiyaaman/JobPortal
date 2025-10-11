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
using Microsoft.Extensions.Logging;

namespace JobPortal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<JobsController> _logger;

        public JobsController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<JobsController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
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

        //public IActionResult Create()
        //{
        //    return View(new Job());
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(Job job, IFormFile companyLogo)
        //{
        //    if (companyLogo != null && companyLogo.Length > 0 && !IsLogoExtensionAllowed(companyLogo))
        //    {
        //        ModelState.AddModelError(string.Empty, "Please upload a logo in PNG, JPG, or SVG format.");
        //        return View(job);
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return View(job);
        //    }

        //    if (companyLogo != null && companyLogo.Length > 0)
        //    {
        //        try
        //        {
        //            job.CompanyLogoPath = await SaveLogo(companyLogo);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Admin Create: Failed to save company logo for job '{Title}'", job?.Title);
        //            ModelState.AddModelError(string.Empty, "We couldn't save the logo. Please try again or use a different image.");
        //            return View(job);
        //        }
        //    }

        //    job.PostedAt = DateTime.UtcNow;
        //    _context.Jobs.Add(job);
        //    await _context.SaveChangesAsync();
        //    TempData["Success"] = "Job created successfully.";
        //    return RedirectToAction(nameof(Index));
        //}

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
                try
                {
                    existingJob.CompanyLogoPath = await SaveLogo(companyLogo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Admin Edit: Failed to save company logo for job Id {JobId}", id);
                    ModelState.AddModelError(string.Empty, "We couldn't save the new logo. Please try again.");
                    return View(existingJob);
                }
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
            // Validate content length (basic guard in addition to request limits)
            if (logoFile.Length <= 0) throw new InvalidOperationException("Logo file is empty.");

            // Resolve a valid web root
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                var contentRoot = _environment.ContentRootPath;
                if (string.IsNullOrEmpty(contentRoot))
                {
                    throw new InvalidOperationException("Cannot resolve a storage root for uploads.");
                }
                webRoot = Path.Combine(contentRoot, "wwwroot");
            }

            var uploadsFolder = Path.Combine(webRoot, "uploads", "logos");
            try
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create admin logo directory {Dir}", uploadsFolder);
                throw;
            }

            var fileName = $"logo_{Guid.NewGuid()}{Path.GetExtension(logoFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 1048576, useAsync: true);
                await logoFile.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save admin logo at {Path}", filePath);
                throw;
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
