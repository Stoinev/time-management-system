using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Identity.Areas.Identity.Data;
using Identity.Data;
using Identity.Models;
using System.Text.Json;

namespace Identity.Controllers
{
    [Authorize]
    public class TimeEntriesController : Controller
    {
        private readonly IdentityDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TimeEntriesController(IdentityDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTimeEntryRequest request)
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // Create new time entry
                var timeEntry = new TimeEntry
                {
                    TaskId = request.TaskId,
                    UserId = userId,
                    Duration = request.Duration,
                    Description = request.Description,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    CreatedDate = DateTime.UtcNow
                };

                _context.TimeEntries.Add(timeEntry);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Time entry saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Get time entries for a specific task
  


        // Update GetByTask to include user info
        [HttpGet]
        public async Task<IActionResult> GetByTask(int id)
        {
            var timeEntries = await _context.TimeEntries
                .Include(te => te.User)
                .Where(te => te.TaskId == id)
                .OrderByDescending(te => te.CreatedDate)
                .Select(te => new
                {
                    te.Id,
                    te.TaskId,
                    te.UserId,
                    te.Duration,
                    te.Description,
                    te.StartTime,
                    te.EndTime,
                    te.CreatedDate,
                    User = new
                    {
                        te.User.UserName,
                        te.User.Email
                    }
                })
                .ToListAsync();

            return Json(timeEntries);
        }

        public class CreateTimeEntryRequest
        {
            public int TaskId { get; set; }
            public int Duration { get; set; }
            public string? Description { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
    }
    // Remove this duplicate method:
    // [HttpGet]
    // public async Task<IActionResult> GetByTask(int taskId)
    // {
    //     var timeEntries = await _context.TimeEntries
    //         .Include(te => te.User)
    //         .Where(te => te.TaskId == taskId)
    //         .OrderByDescending(te => te.CreatedDate)
    //         .ToListAsync();
    //
    //     return Json(timeEntries);
    // }
}