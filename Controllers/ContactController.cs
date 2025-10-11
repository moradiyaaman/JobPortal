using System.Threading.Tasks;
using JobPortal.Data;
using JobPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContactController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ContactMessage
            {
                PreferredContactDate = System.DateTime.UtcNow.Date
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.UserId = _userManager.GetUserId(User);
            _context.ContactMessages.Add(model);
            await _context.SaveChangesAsync();
            ViewData["Success"] = "Thanks! Your message has been sent.";
            ModelState.Clear();
            return View(new ContactMessage
            {
                PreferredContactDate = System.DateTime.UtcNow.Date
            });
        }
    }
}
