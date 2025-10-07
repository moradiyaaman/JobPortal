using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedAccountType = model.AccountType?.Trim();
            var isProviderAccount = string.Equals(normalizedAccountType, "Provider", StringComparison.OrdinalIgnoreCase);

            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Username is already taken.");
                return View(model);
            }

            existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Email is already registered.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address,
                Mobile = model.Mobile,
                Country = model.Country,
                IsProvider = isProviderAccount,
                CompanyName = isProviderAccount ? model.CompanyName?.Trim() : null,
                CompanyWebsite = isProviderAccount ? model.CompanyWebsite?.Trim() : null,
                CompanyLocation = isProviderAccount ? model.CompanyLocation?.Trim() : null,
                CompanyDescription = isProviderAccount ? model.CompanyDescription?.Trim() : null
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (isProviderAccount)
                {
                    if (!await _roleManager.RoleExistsAsync("Provider"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Provider"));
                    }
                    await _userManager.AddToRoleAsync(user, "Provider");
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                if (isProviderAccount)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Provider" });
                }
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApplicationUser user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
            if (user == null && model.UserNameOrEmail.Contains("@"))
            {
                user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                if (await _userManager.IsInRoleAsync(user, "Admin") || user.IsAdmin)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                if (user.IsProvider || await _userManager.IsInRoleAsync(user, "Provider"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Provider" });
                }

                return RedirectToAction("Index", "Jobs");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var isProvider = user.IsProvider || await _userManager.IsInRoleAsync(user, "Provider");

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Address = user.Address,
                Mobile = user.Mobile,
                Country = user.Country,
                Headline = user.Headline,
                Summary = user.Summary,
                Education = user.Education,
                Experience = user.Experience,
                Skills = user.Skills,
                ExistingResumeFileName = user.ResumeFileName,
                IsProvider = isProvider,
                CompanyName = user.CompanyName,
                CompanyWebsite = user.CompanyWebsite,
                CompanyLocation = user.CompanyLocation,
                CompanyDescription = user.CompanyDescription,
                ExistingCompanyLogoPath = user.CompanyLogoPath
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            model.ExistingResumeFileName = user.ResumeFileName;
            var isProviderAccount = user.IsProvider || await _userManager.IsInRoleAsync(user, "Provider");
            model.IsProvider = isProviderAccount;
            model.ExistingCompanyLogoPath = user.CompanyLogoPath;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (isProviderAccount)
            {
                if (string.IsNullOrWhiteSpace(model.CompanyName))
                {
                    ModelState.AddModelError(nameof(model.CompanyName), "Company name is required for provider profiles.");
                }

                if (string.IsNullOrWhiteSpace(model.CompanyLocation))
                {
                    ModelState.AddModelError(nameof(model.CompanyLocation), "Company location is required for provider profiles.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
            }

            user.FullName = model.FullName;
            user.Address = model.Address;
            user.Mobile = model.Mobile;
            user.Country = model.Country;
            user.Headline = model.Headline;
            user.Summary = model.Summary;
            user.Education = model.Education;
            user.Experience = model.Experience;
            user.Skills = model.Skills;

            if (isProviderAccount)
            {
                user.CompanyName = model.CompanyName?.Trim();
                user.CompanyWebsite = model.CompanyWebsite?.Trim();
                user.CompanyLocation = model.CompanyLocation?.Trim();
                user.CompanyDescription = model.CompanyDescription?.Trim();
                user.IsProvider = true;
            }

            if (model.ResumeFile != null && model.ResumeFile.Length > 0)
            {
                if (!IsResumeExtensionAllowed(model.ResumeFile))
                {
                    ModelState.AddModelError("ResumeFile", "Invalid file type. Only PDF, DOC, or DOCX allowed.");
                    return View(model);
                }

                if (!IsResumeFileSizeAllowed(model.ResumeFile))
                {
                    ModelState.AddModelError("ResumeFile", "File too large. Please upload a file within the allowed size.");
                    return View(model);
                }

                var webRoot = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    var contentRoot = _environment.ContentRootPath;
                    if (string.IsNullOrEmpty(contentRoot))
                    {
                        _logger.LogError("Both WebRootPath and ContentRootPath are unavailable when saving resume for user {UserId}", user.Id);
                        ModelState.AddModelError("ResumeFile", "The server cannot determine where to store your resume. Please contact support.");
                        return View(model);
                    }

                    webRoot = Path.Combine(contentRoot, "wwwroot");
                }

                var uploadsFolder = Path.Combine(webRoot, "uploads", "resumes");

                try
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                catch (IOException ioEx)
                {
                    _logger.LogError(ioEx, "Failed to create resume upload directory {Directory}", uploadsFolder);
                    ModelState.AddModelError("ResumeFile", "We couldn't prepare the storage folder for your resume. Please try again later.");
                    return View(model);
                }
                catch (UnauthorizedAccessException accessEx)
                {
                    _logger.LogError(accessEx, "Permission issue while creating resume directory {Directory}", uploadsFolder);
                    ModelState.AddModelError("ResumeFile", "The server does not have permission to store your resume. Please contact support.");
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while creating resume directory {Directory}", uploadsFolder);
                    ModelState.AddModelError("ResumeFile", "An unexpected error occurred while preparing to save your resume. Please try again.");
                    return View(model);
                }

                var fileName = $"resume_{user.Id}{Path.GetExtension(model.ResumeFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                try
                {
                    using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 1048576, useAsync: true);
                    await model.ResumeFile.CopyToAsync(stream);
                }
                catch (IOException ioEx)
                {
                    _logger.LogError(ioEx, "Failed to save resume for user {UserId} at {Path}", user.Id, filePath);
                    ModelState.AddModelError("ResumeFile", "We couldn't save your resume file. Please try again in a moment.");
                    return View(model);
                }
                catch (UnauthorizedAccessException accessEx)
                {
                    _logger.LogError(accessEx, "Permission issue while saving resume for user {UserId} at {Path}", user.Id, filePath);
                    ModelState.AddModelError("ResumeFile", "The server couldn't store your resume due to a permission issue. Please contact support.");
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while saving resume for user {UserId} at {Path}", user.Id, filePath);
                    ModelState.AddModelError("ResumeFile", "An unexpected error occurred while saving your resume. Please try again.");
                    return View(model);
                }

                user.ResumeFileName = $"/uploads/resumes/{fileName}";
            }

            if (isProviderAccount && model.CompanyLogoFile != null && model.CompanyLogoFile.Length > 0)
            {
                if (!IsCompanyLogoExtensionAllowed(model.CompanyLogoFile))
                {
                    ModelState.AddModelError(nameof(model.CompanyLogoFile), "Invalid file type. Only PNG, JPG, JPEG, or SVG allowed.");
                    return View(model);
                }

                if (model.CompanyLogoFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(nameof(model.CompanyLogoFile), "Logo file too large. Please upload an image up to 2 MB.");
                    return View(model);
                }

                var webRoot = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRoot))
                {
                    var contentRoot = _environment.ContentRootPath;
                    if (string.IsNullOrEmpty(contentRoot))
                    {
                        _logger.LogError("Both WebRootPath and ContentRootPath are unavailable when saving company logo for user {UserId}", user.Id);
                        ModelState.AddModelError(nameof(model.CompanyLogoFile), "The server cannot determine where to store your logo. Please contact support.");
                        return View(model);
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
                    _logger.LogError(ex, "Failed to create provider logo directory {Directory}", uploadsFolder);
                    ModelState.AddModelError(nameof(model.CompanyLogoFile), "We couldn't prepare the storage folder for your logo. Please try again later.");
                    return View(model);
                }

                var fileExtension = Path.GetExtension(model.CompanyLogoFile.FileName);
                var fileName = $"company_{user.Id}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                try
                {
                    using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 1048576, useAsync: true);
                    await model.CompanyLogoFile.CopyToAsync(stream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save provider logo for user {UserId} at {Path}", user.Id, filePath);
                    ModelState.AddModelError(nameof(model.CompanyLogoFile), "We couldn't save your logo at this time. Please try again.");
                    return View(model);
                }

                user.CompanyLogoPath = $"/uploads/logos/{fileName}";
                model.ExistingCompanyLogoPath = user.CompanyLogoPath;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (isProviderAccount)
                {
                    var providerJobs = await _context.Jobs
                        .Where(j => j.ProviderId == user.Id)
                        .ToListAsync();

                    foreach (var job in providerJobs)
                    {
                        job.CompanyName = user.CompanyName ?? user.FullName;
                        job.CompanyWebsite = user.CompanyWebsite;
                        job.CompanyLogoPath = user.CompanyLogoPath;
                        job.ProviderDisplayName = user.CompanyName ?? user.FullName;
                        job.ProviderSummary = user.CompanyDescription;
                    }

                    if (providerJobs.Count > 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }

                model.ExistingResumeFileName = user.ResumeFileName;
                model.ExistingCompanyLogoPath = user.CompanyLogoPath;
                model.IsProvider = isProviderAccount;
                ViewData["Success"] = "Profile updated successfully.";
                return View(model);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private bool IsResumeExtensionAllowed(IFormFile file)
        {
            var allowedExtensions = _configuration["ResumeUpload:AllowedExtensions"]?.Split(',') ?? new[] { ".pdf", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Any(ext => ext.Trim().ToLowerInvariant() == extension);
        }

        private bool IsResumeFileSizeAllowed(IFormFile file)
        {
            if (!int.TryParse(_configuration["ResumeUpload:MaxFileSizeMb"], out int maxSizeMb))
            {
                maxSizeMb = 5;
            }

            return file.Length <= maxSizeMb * 1024 * 1024;
        }

        private static readonly string[] AllowedLogoExtensions = new[] { ".png", ".jpg", ".jpeg", ".svg" };

        private bool IsCompanyLogoExtensionAllowed(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedLogoExtensions.Contains(extension);
        }
    }
}
