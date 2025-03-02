using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JobsServer.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Find the project directory path
            var projectDir = Directory.GetCurrentDirectory();

            // Get the configuration from appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(projectDir, "appsettings.json"), optional: false)
                .Build();

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            return new ApplicationDbContext(builder.Options);
        }
    }
}
