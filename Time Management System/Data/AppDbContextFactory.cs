using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Time_Management_System.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseMySql("Server=localhost;Database=timemanagementdb;User=root;Password=0000;",
                new MySqlServerVersion(new Version(8, 0, 41)));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}