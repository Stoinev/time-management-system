using Identity.Areas.Identity.Data;
using Identity.Data;
using Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Identity.Models.TaskStatus;

namespace Identity.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly IdentityDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(IdentityDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            // Load tasks created by this user (or adjust logic as needed)
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedBy)
                .Where(t => t.CreatedById == userId)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            var projects = await _context.Projects
                .Where(p => p.CreatedById == userId)
                .ToListAsync();

            ViewData["Projects"] = projects;

            return View(tasks);
        }

       


        // GET: Tasks/Create
        public async Task<IActionResult> Create()
        {
            // Load projects for dropdown - only projects created by current user
            var userId = _userManager.GetUserId(User);
            var projects = await _context.Projects
                .Where(p => p.CreatedById == userId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");

            // Load users for AssignedUser dropdown (optional)
            var users = await _userManager.Users.ToListAsync();
            ViewData["AssignedUserId"] = new SelectList(users, "Id", "UserName");

            // Load enums for Priority and Status dropdowns
            ViewData["Priorities"] = Enum.GetValues(typeof(TaskPriority))
                .Cast<TaskPriority>()
                .Select(p => new SelectListItem(p.ToString(), p.ToString()))
                .ToList();

            ViewData["Statuses"] = Enum.GetValues(typeof(TaskStatus))
                .Cast<TaskStatus>()
                .Select(s => new SelectListItem(s.ToString(), s.ToString()))
                .ToList();

            return View();
        }

        // POST: Tasks/Create
         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Create(TaskItem task)
         {

            ModelState.Remove("CreatedById");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("Project");
            ModelState.Remove("AssignedUser");

             if (!ModelState.IsValid)
             {
                 // Re-populate dropdowns if validation fails
                 var userId = _userManager.GetUserId(User);
                 var projects = await _context.Projects
                     .Where(p => p.CreatedById == userId)
                     .OrderBy(p => p.Name)
                     .ToListAsync();
                 ViewData["ProjectId"] = new SelectList(projects, "Id", "Name", task.ProjectId);

                 var users = await _userManager.Users.ToListAsync();
                 ViewData["AssignedUserId"] = new SelectList(users, "Id", "UserName", task.AssignedUserId);

                 ViewData["Priorities"] = Enum.GetValues(typeof(TaskPriority))
                     .Cast<TaskPriority>()
                     .Select(p => new SelectListItem(p.ToString(), p.ToString(), p == task.Priority))
                     .ToList();

                 ViewData["Statuses"] = Enum.GetValues(typeof(TaskStatus))
                     .Cast<TaskStatus>()
                     .Select(s => new SelectListItem(s.ToString(), s.ToString(), s == task.Status))
                     .ToList();

                 return View(task);
             }

             // Set metadata fields
             task.CreatedById = _userManager.GetUserId(User);
             task.CreatedDate = DateTime.UtcNow;
             task.UpdatedDate = DateTime.UtcNow;

             _context.Tasks.Add(task);
             await _context.SaveChangesAsync();

             return RedirectToAction(nameof(Index));
         }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            // Populate dropdowns
            var userId = _userManager.GetUserId(User);
            var projects = await _context.Projects
                .Where(p => p.CreatedById == userId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name", task.ProjectId);

            var users = await _userManager.Users.ToListAsync();
            ViewData["AssignedUserId"] = new SelectList(users, "Id", "UserName", task.AssignedUserId);

            ViewData["Priorities"] = Enum.GetValues(typeof(TaskPriority))
                .Cast<TaskPriority>()
                .Select(p => new SelectListItem(p.ToString(), p.ToString(), p == task.Priority))
                .ToList();

            ViewData["Statuses"] = Enum.GetValues(typeof(TaskStatus))
                .Cast<TaskStatus>()
                .Select(s => new SelectListItem(s.ToString(), s.ToString(), s == task.Status))
                .ToList();

            return View(task);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem task)
        {

            ModelState.Remove("CreatedById");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("Project");
            ModelState.Remove("AssignedUser");


            if (id != task.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                // Repopulate dropdowns
                var userId = _userManager.GetUserId(User);
                var projects = await _context.Projects
                    .Where(p => p.CreatedById == userId)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                ViewData["ProjectId"] = new SelectList(projects, "Id", "Name", task.ProjectId);

                var users = await _userManager.Users.ToListAsync();
                ViewData["AssignedUserId"] = new SelectList(users, "Id", "UserName", task.AssignedUserId);

                ViewData["Priorities"] = Enum.GetValues(typeof(TaskPriority))
                    .Cast<TaskPriority>()
                    .Select(p => new SelectListItem(p.ToString(), p.ToString(), p == task.Priority))
                    .ToList();

                ViewData["Statuses"] = Enum.GetValues(typeof(TaskStatus))
                    .Cast<TaskStatus>()
                    .Select(s => new SelectListItem(s.ToString(), s.ToString(), s == task.Status))
                    .ToList();

                return View(task);
            }

            try
            {
                var existingTask = await _context.Tasks.FindAsync(id);
                if (existingTask == null)
                    return NotFound();

                // Update fields
                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.ProjectId = task.ProjectId;
                existingTask.AssignedUserId = task.AssignedUserId;
                existingTask.Priority = task.Priority;
                existingTask.Status = task.Status;
                existingTask.EstimatedHours = task.EstimatedHours;
                existingTask.UpdatedDate = DateTime.UtcNow;

                _context.Update(existingTask);
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine("Warning: SaveChanges returned 0 rows affected.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred updating the task.");
            }

            // On error, repopulate dropdowns and return view
            var userIdFallback = _userManager.GetUserId(User);
            var projectsFallback = await _context.Projects
                .Where(p => p.CreatedById == userIdFallback)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewData["ProjectId"] = new SelectList(projectsFallback, "Id", "Name", task.ProjectId);

            var usersFallback = await _userManager.Users.ToListAsync();
            ViewData["AssignedUserId"] = new SelectList(usersFallback, "Id", "UserName", task.AssignedUserId);

            ViewData["Priorities"] = Enum.GetValues(typeof(TaskPriority))
                .Cast<TaskPriority>()
                .Select(p => new SelectListItem(p.ToString(), p.ToString(), p == task.Priority))
                .ToList();

            ViewData["Statuses"] = Enum.GetValues(typeof(TaskStatus))
                .Cast<TaskStatus>()
                .Select(s => new SelectListItem(s.ToString(), s.ToString(), s == task.Status))
                .ToList();

            return View(task);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return View(task);
        }
        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Tasks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return View(task);
        }

    }
}