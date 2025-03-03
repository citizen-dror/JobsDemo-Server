using JobsServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) 
        { }

        public DbSet<Job> Jobs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Job entity configuration
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.JobName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Priority).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.JobData).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

                // Indexes for performance
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.ScheduledTime);
                entity.HasIndex(e => e.AssignedWorker);
            });

            // Worker node entity configuration
            modelBuilder.Entity<WorkerNode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).HasConversion<int>();

                // Indexes for performance
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.LastHeartbeat);
            });

            // Job execution log entity configuration
            modelBuilder.Entity<JobExecutionLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.JobId).IsRequired();
                entity.Property(e => e.LogLevel).HasConversion<int>();
                entity.Property(e => e.Message).IsRequired();

                // Index for job correlation
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => e.Timestamp);
            });
        }
    }
}
