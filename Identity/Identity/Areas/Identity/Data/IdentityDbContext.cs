using Identity.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data;

public class IdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TaskItem>(entity =>
        {
            entity.Property(t => t.Priority)
                  .HasConversion<string>();

            entity.Property(t => t.Status)
                  .HasConversion<string>();
        });
    }
}
