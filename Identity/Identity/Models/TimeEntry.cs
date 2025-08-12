using Identity.Areas.Identity.Data;

namespace Identity.Models
{
    public class TimeEntry
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public TaskItem Task { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public int? Duration { get; set; } // in minutes

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }
    }


}