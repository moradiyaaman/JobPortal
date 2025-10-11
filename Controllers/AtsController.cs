using System.Threading.Tasks;
using JobPortal.Models;
using JobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobPortal.Controllers
{
    [Authorize]
    public class AtsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAtsScorer _ats;

        public AtsController(UserManager<ApplicationUser> userManager, IAtsScorer ats)
        {
            _userManager = userManager;
            _ats = ats;
        }

        [HttpGet]
        public async Task<IActionResult> Resume()
        {
            var user = await _userManager.GetUserAsync(User);
            var res = await _ats.ScoreResumeAsync(user);
            return View(res);
        }
    }
}
