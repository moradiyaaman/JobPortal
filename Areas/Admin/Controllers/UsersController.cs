using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Models;
using JobPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var adminRoleId = await GetRoleIdAsync("Admin");
            var providerRoleId = await GetRoleIdAsync("Provider");

            var userRoles = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId || ur.RoleId == providerRoleId)
                .ToListAsync();

            var adminUserIds = userRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).ToHashSet();
            var providerUserIds = userRoles.Where(ur => ur.RoleId == providerRoleId).Select(ur => ur.UserId).ToHashSet();

            var jobCounts = await _context.Jobs
                .Where(j => j.ProviderId != null)
                .GroupBy(j => j.ProviderId)
                .Select(g => new { ProviderId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ProviderId, x => x.Count);

            var applicationCounts = await _context.JobApplications
                .GroupBy(a => a.ApplicantId)
                .Select(g => new { ApplicantId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ApplicantId, x => x.Count);

            var users = await _context.Users
                .OrderBy(u => u.UserName)
                .AsNoTracking()
                .ToListAsync();

            var viewModel = users.Select(u => new AdminUserListItemViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = u.FullName,
                IsAdmin = u.IsAdmin || adminUserIds.Contains(u.Id),
                IsProvider = u.IsProvider || providerUserIds.Contains(u.Id),
                EmailConfirmed = u.EmailConfirmed,
                IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow,
                CreatedAt = u.CreatedAt,
                JobsPosted = jobCounts.TryGetValue(u.Id, out var jobCount) ? jobCount : 0,
                ApplicationsSubmitted = applicationCounts.TryGetValue(u.Id, out var appCount) ? appCount : 0
            }).ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var adminRoleId = await GetRoleIdAsync("Admin");
            var providerRoleId = await GetRoleIdAsync("Provider");

            var viewModel = new AdminUserDetailsViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Mobile = user.Mobile,
                Country = user.Country,
                Address = user.Address,
                Headline = user.Headline,
                Summary = user.Summary,
                IsAdmin = user.IsAdmin || await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == adminRoleId),
                IsProvider = user.IsProvider || await _context.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == providerRoleId),
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow,
                CreatedAt = user.CreatedAt,
                CompanyName = user.CompanyName,
                CompanyWebsite = user.CompanyWebsite,
                CompanyLocation = user.CompanyLocation,
                CompanyDescription = user.CompanyDescription,
                JobsPosted = await _context.Jobs.CountAsync(j => j.ProviderId == user.Id),
                ApplicationsSubmitted = await _context.JobApplications.CountAsync(a => a.ApplicantId == user.Id)
            };

            return View(viewModel);
        }

        public IActionResult CreateAdmin()
        {
            return View(new AdminCreateAdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(AdminCreateAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _userManager.FindByNameAsync(model.UserName) != null)
            {
                ModelState.AddModelError(nameof(model.UserName), "Username is already taken.");
                return View(model);
            }

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email is already registered.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                IsAdmin = true,
                Country = "Remote"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            await _userManager.AddToRoleAsync(user, "Admin");
            TempData["Success"] = "Admin user created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Info"] = $"{user.UserName} is already an administrator.";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.AddToRoleAsync(user, "Admin");
            user.IsAdmin = true;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"{user.UserName} is now an administrator.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockout(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                TempData["Error"] = "You cannot change your own lockout status.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow)
            {
                user.LockoutEnd = null;
                TempData["Success"] = $"{user.UserName} has been re-enabled.";
            }
            else
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(10);
                TempData["Success"] = $"{user.UserName} has been disabled.";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var providerJobs = await _context.Jobs.Where(j => j.ProviderId == user.Id).ToListAsync();
            if (providerJobs.Count > 0)
            {
                _context.Jobs.RemoveRange(providerJobs);
                await _context.SaveChangesAsync();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = $"{user.UserName} has been removed.";
            }
            else
            {
                TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GetRoleIdAsync(string roleName)
        {
            var role = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == roleName);
            return role?.Id;
        }
    }
}
