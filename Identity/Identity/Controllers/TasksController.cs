using Identity.Areas.Identity.Data;
using Identity.Data;
using Identity.Models;
using Identity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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

        public async Task<IActionResult> Index(List<int>? tagIds = null)
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedBy)
                .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
                .Where(t => t.CreatedById == userId);

            if (tagIds != null && tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    query = query.Where(t => t.TaskTags.Any(tt => tt.TagId == tagId));
                }
            }

            var tasks = await query
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            var projects = await _context.Projects
                .Where(p => p.CreatedById == userId)
                .ToListAsync();

            ViewBag.AvailableTags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
            ViewBag.SelectedTagIds = tagIds ?? new List<int>();
            ViewData["Projects"] = projects;

            return View(tasks);
        }




        // GET: Tasks/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);
            var projects = await _context.Projects
                .Where(p => p.CreatedById == userId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");

            var users = await _userManager.Users.ToListAsync();
            ViewData["AssignedUserId"] = new SelectList(users, "Id", "UserName");

            ViewData["Priorities"] = Enum.GetValues(typeof(TaskPriority))
                .Cast<TaskPriority>()
                .Select(p => new SelectListItem(p.ToString(), p.ToString()))
                .ToList();

            ViewData["Statuses"] = Enum.GetValues(typeof(TaskStatus))
                .Cast<TaskStatus>()
                .Select(s => new SelectListItem(s.ToString(), s.ToString()))
                .ToList();


            ViewData["AvailableTags"] = new MultiSelectList(
                await _context.Tags.OrderBy(t => t.Name).ToListAsync(),
                "Id",
                "Name"
            );

            return View();
        }

        // POST: Tasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem task, List<int>? SelectedTagIds)
        {
            ModelState.Remove("CreatedById");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("Project");
            ModelState.Remove("AssignedUser");
            ModelState.Remove("SelectedTagIds");

            if (!ModelState.IsValid)
            {
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

                ViewData["AvailableTags"] = new MultiSelectList(
                    await _context.Tags.OrderBy(t => t.Name).ToListAsync(),
                    "Id",
                    "Name",
                    SelectedTagIds
                );

                return View(task);
            }

            task.CreatedById = _userManager.GetUserId(User);
            task.CreatedDate = DateTime.UtcNow;
            task.UpdatedDate = DateTime.UtcNow;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            if (SelectedTagIds != null && SelectedTagIds.Any())
            {
                foreach (var tagId in SelectedTagIds)
                {
                    _context.TaskTags.Add(new TaskTag { TaskId = task.Id, TagId = tagId });
                }
                await _context.SaveChangesAsync();
            }

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
                .Include(t => t.TaskTags)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

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

            ViewData["AvailableTags"] = new MultiSelectList(
                await _context.Tags.OrderBy(t => t.Name).ToListAsync(),
                "Id",
                "Name",
                task.TaskTags.Select(tt => tt.TagId).ToList()
            );
            ViewData["SelectedTagIds"] = task.TaskTags.Select(tt => tt.TagId).ToList();

            return View(task);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem task, List<int>? SelectedTagIds)
        {
            ModelState.Remove("CreatedById");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("Project");
            ModelState.Remove("AssignedUser");
            ModelState.Remove("SelectedTagIds");

            if (id != task.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
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

                ViewData["AvailableTags"] = new MultiSelectList(
                    await _context.Tags.OrderBy(t => t.Name).ToListAsync(),
                    "Id",
                    "Name",
                    SelectedTagIds
                );

                return View(task);
            }

            try
            {
                var existingTask = await _context.Tasks
                    .Include(t => t.TaskTags)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (existingTask == null)
                    return NotFound();

                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.ProjectId = task.ProjectId;
                existingTask.AssignedUserId = task.AssignedUserId;
                existingTask.Priority = task.Priority;
                existingTask.Status = task.Status;
                existingTask.EstimatedHours = task.EstimatedHours;
                existingTask.UpdatedDate = DateTime.UtcNow;

                _context.TaskTags.RemoveRange(existingTask.TaskTags);

                if (SelectedTagIds != null && SelectedTagIds.Any())
                {
                    foreach (var tagId in SelectedTagIds)
                    {
                        _context.TaskTags.Add(new TaskTag { TaskId = existingTask.Id, TagId = tagId });
                    }
                }

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

            ViewData["AvailableTags"] = new MultiSelectList(
                await _context.Tags.OrderBy(t => t.Name).ToListAsync(),
                "Id",
                "Name",
                SelectedTagIds
            );

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
                .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return View(task);
        }

    }
}