using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Data;
using Identity.Models;
using Identity.Areas.Identity.Data;

namespace Identity.Controllers
{
    public class BoardController : Controller
    {
        private readonly IdentityDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BoardController(IdentityDbContext context, UserManager<ApplicationUser> userManager)
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

            // Create DTOs to avoid circular references
            var taskDtos = tasks.Select(t => new
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                ProjectId = t.ProjectId,
                Project = t.Project != null ? new { Id = t.Project.Id, Name = t.Project.Name } : null,
                AssignedUserId = t.AssignedUserId,
                AssignedUser = t.AssignedUser != null ? new
                {
                    Id = t.AssignedUser.Id,
                    UserName = t.AssignedUser.UserName,
                    Email = t.AssignedUser.Email
                } : null,
                Priority = t.Priority.ToString(),
                Status = t.Status.ToString(),
                CreatedDate = t.CreatedDate,
                UpdatedDate = t.UpdatedDate,
                TaskTags = t.TaskTags.Select(tt => new {
                    Tag = new
                    {
                        Id = tt.Tag.Id,
                        Name = tt.Tag.Name,
                        ColorCode = tt.Tag.ColorCode
                    }
                }).ToList()
            }).ToList();

            ViewBag.AvailableTags = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
            ViewBag.SelectedTagIds = tagIds ?? new List<int>();
            ViewData["Projects"] = projects;
            ViewData["UserProjects"] = projects;

            return View(taskDtos);
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuickTask([FromBody] QuickTaskModel model)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                var task = new TaskItem
                {
                    Title = model.Title,
                    ProjectId = model.ProjectId,
                    Status = model.Status,
                    Priority = TaskPriority.Low,
                    CreatedById = userId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                return Json(new { success = true, taskId = task.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Action to get task details for modal
        public async Task<IActionResult> TaskDetails(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedBy)
                .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return PartialView("_TaskDetailsModal", task);
        }
    }
}