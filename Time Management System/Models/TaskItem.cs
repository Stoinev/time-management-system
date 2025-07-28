using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Time_Management_System.Models
{
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Testing,
        Done
    }

    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public Project Project { get; set; }

        public int? AssignedUserId { get; set; }

        public User AssignedUser { get; set; }

        [Required]
        public int CreatedById { get; set; }

        public User CreatedBy { get; set; }

        public decimal? EstimatedHours { get; set; }

        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }
}
