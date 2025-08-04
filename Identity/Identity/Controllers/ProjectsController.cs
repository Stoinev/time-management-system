using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Areas.Identity.Data;
using Identity.Data;
using Identity.Models;

namespace Identity.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IdentityDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectsController(IdentityDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Projects/Manage
    public async Task<IActionResult> Manage()
    {
        var userId = _userManager.GetUserId(User);
        var projects = await _context.Projects
            .Where(p => p.CreatedById == userId)
            .Include(p => p.CreatedBy)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

        return View(projects);
    }

    // GET: /Projects/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Projects/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create ([Bind("Name,Description")] Project project)
    {
        ModelState.Remove("CreatedById");
        ModelState.Remove("CreatedBy");

        if (!ModelState.IsValid)
        {
            return View(project);
        }

        // Get current user
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError("", "User authentication error");
            return View(project);
        }

        // Set project properties
        project.CreatedById = userId;
        project.CreatedDate = DateTime.UtcNow;
        project.IsActive = true;

        Console.WriteLine($"About to save project: Name='{project.Name}', CreatedById='{project.CreatedById}'");

        try
        {
            _context.Add(project);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return RedirectToAction(nameof(Manage));
            }

            ModelState.AddModelError("", "Project could not be saved. Try again.");
        }
            catch (Exception ex)
        {
            // Log to console or better: use ILogger
            Console.Error.WriteLine($"[ERROR] Failed to save project: {ex.Message}");
            ModelState.AddModelError("", "An error occurred while saving the project.");
        }

        Console.WriteLine("Returning to Create view with errors");
        return View(project);
    }




    // GET: /Projects/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var project = await _context.Projects
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return NotFound();

        return View(project);
    }



    // GET: Projects/Edit/5
     public async Task<IActionResult> Edit(int? id)
     {
         if (id == null) return NotFound();

         var project = await _context.Projects
             .FirstOrDefaultAsync(p => p.Id == id);

         if (project == null) return NotFound();

         // Authorization: only allow editing your own projects
         var userId = _userManager.GetUserId(User);
         if (project.CreatedById != userId)
             return Forbid();

         return View(project);
     }

     // POST: Projects/Edit/5
     [HttpPost]
     [ValidateAntiForgeryToken]
     public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsActive")] Project project)
     {
        ModelState.Remove("CreatedById");

        if (id != project.Id)
             return NotFound();

         // Ensure the user owns the project
         var existingProject = await _context.Projects
             .FirstOrDefaultAsync(p => p.Id == id);

         if (existingProject == null)
             return NotFound();

         var userId = _userManager.GetUserId(User);
         if (existingProject.CreatedById != userId)
             return Forbid();

         if (!ModelState.IsValid)
             return View(project);

         // Update only allowed fields
         existingProject.Name = project.Name;
         existingProject.Description = project.Description;
         existingProject.IsActive = project.IsActive;

        try
        {
            if (existingProject == null)
            {
                Console.WriteLine($"Project with ID {id} not found in DB.");
                return NotFound();
            }

            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.IsActive = project.IsActive;

            _context.Update(existingProject);
            var result = await _context.SaveChangesAsync();

            Console.WriteLine($"SaveChanges result: {result} rows affected");
            return RedirectToAction(nameof(Manage));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DATABASE ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            ModelState.AddModelError("", $"Database error: {ex.Message}");
            return View(project);
        }
    }


    // GET: Projects/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var project = await _context.Projects
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound();

        return View(project);
    }


    // POST: Projects/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        ModelState.Remove("CreatedById");
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
            return NotFound();

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Manage));
    }


    private bool ProjectExists(int id)
    {
        return _context.Projects.Any(e => e.Id == id);
    }
}
