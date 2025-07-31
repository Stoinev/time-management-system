using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<Identity.Areas.Identity.Data.ApplicationUser> _userManager;
        public AdminController(UserManager<Identity.Areas.Identity.Data.ApplicationUser> userManager)
        {
            this._userManager = userManager;
        }
        public async Task<IActionResult> ListUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userManager.DeleteAsync(user);

            return RedirectToAction("ListUsers");
        }
    }
}
