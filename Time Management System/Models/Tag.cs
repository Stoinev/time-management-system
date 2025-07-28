using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Time_Management_System.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        [Required]
        [StringLength(7)]
        public string ColorCode { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }
}
