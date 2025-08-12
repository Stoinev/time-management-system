using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public class Tag
    {
        public Tag()
        {
            CreatedDate = DateTime.UtcNow;
        }
        public int Id { get; set; }

        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(30, ErrorMessage = "Tag name cannot exceed 30 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Color code is required")]
        [RegularExpression(@"^#([A-Fa-f0-9]{6})$", ErrorMessage = "Color must be in hex format (e.g., #FF0000)")]
        [StringLength(7, MinimumLength = 7)]
        public string ColorCode { get; set; }

        public DateTime CreatedDate { get; set; }
        

        // Navigation property
        public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    }
}