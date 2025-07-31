using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Time_Management_System.Models
{
    public enum UserRole
    {
        Admin,
        User
    }

    public class ApplicationUser : IdentityUser<int>
    {
        //[Key]
        //public int Id { get; set; }

      //  [Required]
      //  [MaxLength(50)]
      //  public string Username { get; set; }

     //   [Required]
      //  [MaxLength(100)]
     //   [EmailAddress]
     //   public string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

       // [Required]
       // public string PasswordHash { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
        public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }
}
