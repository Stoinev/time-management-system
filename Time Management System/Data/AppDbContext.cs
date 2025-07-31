using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Time_Management_System.Models;

namespace Time_Management_System.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext()
        {

        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }


        // public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TaskTag> TaskTags { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }

       /* protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("Server=localhost;Database=timemanagementdb;User=root;Password=0000;",
                    new MySqlServerVersion(new Version(8, 0, 41)));
            }
        }*/

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskTag>()
                .HasKey(tt => new { tt.TaskId, tt.TagId });
            // Project -> User (CreatedBy)
            modelBuilder.Entity<Project>()
                .HasOne(p => p.CreatedBy)
                .WithMany(u => u.CreatedProjects)
                .HasForeignKey(p => p.CreatedById);

            // Task -> Project
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId);

            // Task -> User (Assigned)
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedUserId);

            // TaskTag many-to-many
            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Task)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TaskId);

            modelBuilder.Entity<TaskTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TaskTags)
                .HasForeignKey(tt => tt.TagId);

            // TimeEntry -> Task
            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.Task)
                .WithMany(t => t.TimeEntries)
                .HasForeignKey(te => te.TaskId);

            // TimeEntry -> User
            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.User)
                .WithMany(u => u.TimeEntries)
                .HasForeignKey(te => te.UserId);

            // 📌 Default values and constraints

            modelBuilder.Entity<Project>()
                .Property(p => p.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Status)
                .HasDefaultValue(Time_Management_System.Models.TaskStatus.ToDo);

            modelBuilder.Entity<Tag>()
                .Property(t => t.ColorCode)
                .HasMaxLength(7);
        }
    }
}