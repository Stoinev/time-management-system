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

        /* [HttpPost]
         public async Task<IActionResult> CreateQuickTask([FromBody] QuickTaskModel model)
         {
             try
             {
                 var userId = _userManager.GetUserId(User);

                 var task = new TaskItem
                 {
                     Title = model.Title,
                     ProjectId = model.ProjectId ?? await GetDefaultProjectId(userId),
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
         }*/
        [HttpPost]
        public async Task<IActionResult> CreateQuickTask([FromBody] QuickTaskModel model)
        {
            try
            {
                Console.WriteLine($"=== DEBUG CONTROLLER START ===");
                Console.WriteLine($"Request Content-Type: {Request.ContentType}");

                // Check if model is null first
                if (model == null)
                {
                    Console.WriteLine("ERROR: Model is null");

                    // Try to read the raw request body to see what's being sent
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(Request.Body))
                    {
                        var rawBody = await reader.ReadToEndAsync();
                        Console.WriteLine($"Raw request body: {rawBody}");
                    }

                    return Json(new { success = false, message = "Model is null - check model binding" });
                }

                Console.WriteLine($"Model received successfully");
                Console.WriteLine($"Model.Title: '{model.Title}'");
                Console.WriteLine($"Model.ProjectId: {model.ProjectId}");
                Console.WriteLine($"Model.Status: {model.Status}");

                // Check user authentication
                var userId = _userManager.GetUserId(User);
                Console.WriteLine($"UserId: '{userId}'");

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("ERROR: User not authenticated");
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Validate model properties
                if (string.IsNullOrWhiteSpace(model.Title))
                {
                    Console.WriteLine("ERROR: Title is null or empty");
                    return Json(new { success = false, message = "Title is required" });
                }

                if (!model.ProjectId.HasValue || model.ProjectId.Value <= 0)
                {
                    Console.WriteLine("ERROR: ProjectId is null or invalid");
                    return Json(new { success = false, message = "Valid ProjectId is required" });
                }

                // Check if project exists and user has access
                Console.WriteLine($"Looking for project with ID: {model.ProjectId.Value}");
                var project = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Id == model.ProjectId.Value && p.CreatedById == userId);

                if (project == null)
                {
                    Console.WriteLine($"ERROR: Project not found or access denied. ProjectId: {model.ProjectId.Value}, UserId: {userId}");
                    return Json(new { success = false, message = "Project not found or access denied" });
                }

                Console.WriteLine($"Project found: '{project.Name}'");

                // Parse TaskStatus enum - make sure this matches your enum exactly
                // Replace this block:
                /*
                if (!Enum.TryParse<Models.TaskStatus>(model.Status, out var taskStatus))
                {
                    Console.WriteLine($"ERROR: Invalid status: '{model.Status}'");
                    return Json(new { success = false, message = "Invalid status" });
                }

                Console.WriteLine($"Status parsed successfully: {taskStatus}");
                */

                // With this block:
                var taskStatus = model.Status;
                Console.WriteLine($"Status received: {taskStatus}");

                Console.WriteLine($"Status parsed successfully: {taskStatus}");

                // Create TaskItem
                Console.WriteLine("Creating TaskItem...");
                var task = new TaskItem
                {
                    Title = model.Title.Trim(),
                    ProjectId = model.ProjectId.Value,
                    Status = taskStatus,
                    Priority = TaskPriority.Low,
                    CreatedById = userId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                Console.WriteLine($"TaskItem created - Title: '{task.Title}', ProjectId: {task.ProjectId}, Status: {task.Status}");

                // Add to context
                Console.WriteLine("Adding to context...");
                _context.Tasks.Add(task);

                Console.WriteLine("Saving changes...");
                await _context.SaveChangesAsync();

                Console.WriteLine($"SUCCESS: Task created with ID: {task.Id}");
                return Json(new { success = true, taskId = task.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                }

                return Json(new { success = false, message = ex.Message });
            }
        }
        private async Task<int> GetDefaultProjectId(string userId)
        {
            // Try to find an existing "Inbox" or default project
            var defaultProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.CreatedById == userId && p.Name == "Inbox");

            if (defaultProject == null)
            {
                // Create a default project if none exists
                defaultProject = new Project
                {
                    Name = "Inbox",
                    CreatedById = userId,
                    CreatedDate = DateTime.Now
                };
                _context.Projects.Add(defaultProject);
                await _context.SaveChangesAsync();
            }

            return defaultProject.Id;
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



        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus(int taskId, string newStatus, int newPosition)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var task = await _context.Tasks
                    .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedById == userId);

                if (task == null)
                    return Json(new { success = false, message = "Task not found or access denied" });

                // Parse the string status to your enum
                if (Enum.TryParse<Models.TaskStatus>(newStatus, out var parsedStatus))
                {
                    task.Status = parsedStatus;
                    task.UpdatedDate = DateTime.Now;

                    // Optionally store position if you have a Position field
                    // task.Position = newPosition;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid status value" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}