using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Time_Management_System.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public int CreatedById { get; set; }

        public User CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
