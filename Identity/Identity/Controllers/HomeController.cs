using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Identity.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Identity.Data;

namespace Identity.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityDbContext _context;

    public HomeController(ILogger<HomeController> logger,UserManager<ApplicationUser> userManager, IdentityDbContext context)
    {
        _logger = logger;
        this._userManager = userManager;
        _context = context;
    }

    private void LoadUserProjects()
    {
        var userId = _userManager.GetUserId(User);

        if (!string.IsNullOrEmpty(userId))
        {
            var userProjects = _context.Projects
                .Where(p => p.CreatedById == userId && p.IsActive)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            ViewBag.UserProjects = userProjects;
        }
    }

    public IActionResult Index()
    {
        ViewData["UserID"] = _userManager.GetUserId(this.User);
        return View();
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
