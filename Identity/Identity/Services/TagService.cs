using Identity.Data;
using Identity.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeManagementSystem.Services
{
    public class TagService
    {
        private readonly IdentityDbContext _context;

        public TagService(IdentityDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<Tag>> GetTagsByTaskIdAsync(int taskId)
        {
            return await _context.TaskTags
                .Where(tt => tt.TaskId == taskId)
                .Select(tt => tt.Tag)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task AssignTagsToTaskAsync(int taskId, List<int> tagIds)
        {
            // Remove existing tags
            var existingTags = await _context.TaskTags
                .Where(tt => tt.TaskId == taskId)
                .ToListAsync();

            _context.TaskTags.RemoveRange(existingTags);

            // Add new tags
            if (tagIds != null && tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    _context.TaskTags.Add(new TaskTag
                    {
                        TaskId = taskId,
                        TagId = tagId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public string GetContrastTextColor(string backgroundColor)
        {
            var color = System.Drawing.ColorTranslator.FromHtml(backgroundColor);
            var brightness = (color.R * 299 + color.G * 587 + color.B * 114) / 1000;
            return brightness > 128 ? "#000000" : "#FFFFFF";
        }
    }
}