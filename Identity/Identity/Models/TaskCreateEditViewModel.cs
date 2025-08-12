using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Identity.Models;

namespace Identity.ViewModels
{
    public class TaskCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Task Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a project")]
        [Display(Name = "Project")]
        public int ProjectId { get; set; }

        [Display(Name = "Assigned To")]
        public string? AssignedUserId { get; set; }

        [Display(Name = "Estimated Hours")]
        [Range(0.01, 999.99, ErrorMessage = "Estimated hours must be between 0.01 and 999.99")]
        public decimal? EstimatedHours { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public System.Threading.Tasks.TaskStatus Status { get; set; }

        // Tag Selection
        [Display(Name = "Tags")]
        public List<int> SelectedTagIds { get; set; } = new List<int>();

        // Dropdown Lists for the form
        public SelectList? ProjectList { get; set; }
        public SelectList? UserList { get; set; }
        public MultiSelectList? AvailableTags { get; set; }

        // For display purposes in edit mode
        public string? CreatedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}