using Microsoft.EntityFrameworkCore;
using JobQueueSystem.Core.Enums;
using JobsServer.Infrastructure;
using JobsServer.Domain.Interfaces.APIs;

namespace JobQueueSystem.QueueService.Services
{

    public class JobQueueService : BackgroundService
    {
        private readonly ILogger<JobQueueService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _queueProcessorTimer;
        private Timer _heartbeatTimer;

        public JobQueueService(
            ILogger<JobQueueService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Job Queue Service starting");

            // Timer for processing the job queue (every 10seconds)
            _queueProcessorTimer = new Timer(ProcessQueue, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            // Timer for checking worker heartbeats (every 2 min)
            _heartbeatTimer = new Timer(CheckWorkerHeartbeats, null, TimeSpan.Zero, TimeSpan.FromSeconds(120));

            return Task.CompletedTask;
        }

        private async void ProcessQueue(object state)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var jobUpdateHub = scope.ServiceProvider.GetRequiredService<IJobUpdateHub>();

                // Get all available workers
                var availableWorkers = await dbContext.WorkerNodes
                    .Where(w => w.Status != WorkerStatus.Offline && w.ActiveJobCount < w.ConcurrencyLimit)
                    .ToListAsync();

                if (!availableWorkers.Any())
                {
                    _logger.LogInformation("No available workers found");
                    return;
                }

                // Get pending jobs ordered by priority and scheduled time
                var now = DateTime.UtcNow;
                var pendingJobs = await dbContext.Jobs
                    .Where(j => (j.Status == JobStatus.Pending || j.Status == JobStatus.Scheduled)
                             && (j.ScheduledTime == null || j.ScheduledTime <= now))
                    .OrderByDescending(j => j.Priority)
                    .ThenBy(j => j.ScheduledTime ?? DateTime.MinValue)
                    .Take(availableWorkers.Count * 2) // Get more jobs than workers to allow for prioritization
                    .ToListAsync();

                if (!pendingJobs.Any())
                {
                    return;
                }

                // Get job distributor from the same scope
                var jobDistributor = scope.ServiceProvider.GetRequiredService<JobDistributor>();

                // Distribute jobs to workers
                await jobDistributor.DistributeJobs(pendingJobs, availableWorkers);

                // Notify clients about job status changes
                await jobUpdateHub.BroadcastJobStatusUpdates(pendingJobs.Select(j => j.Id).ToList());
                await jobUpdateHub.BroadcastWorkerStatusUpdates(availableWorkers.Select(w => w.Id).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing job queue");
            }
        }

        private async void CheckWorkerHeartbeats(object state)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var jobUpdateHub = scope.ServiceProvider.GetRequiredService<IJobUpdateHub>();

                // Find workers with stale heartbeats (more than 2 minutes old)
                var staleTime = DateTime.UtcNow.AddMinutes(-2);
                var staleWorkers = await dbContext.WorkerNodes
                    .Where(w => w.Status != WorkerStatus.Offline && w.LastHeartbeat < staleTime)
                    .ToListAsync();

                if (!staleWorkers.Any())
                {
                    return;
                }

                // Mark workers as offline
                foreach (var worker in staleWorkers)
                {
                    _logger.LogWarning($"Worker {worker.Name} ({worker.Id}) appears to be offline. Last heartbeat: {worker.LastHeartbeat}");
                    worker.Status = WorkerStatus.Offline;

                    // Get jobs assigned to this worker and mark them for retry
                    var assignedJobs = await dbContext.Jobs
                        .Where(j => j.AssignedWorker == worker.Id && j.Status == JobStatus.InProgress)
                        .ToListAsync();

                    foreach (var job in assignedJobs)
                    {
                        job.Status = job.RetryCount < job.MaxRetries ? JobStatus.Retrying : JobStatus.Failed;
                        job.ErrorMessage = $"Worker went offline while processing job";
                        job.AssignedWorker = null;
                        _logger.LogWarning($"Job {job.Id} ({job.JobName}) marked for retry due to worker offline");
                    }
                }

                await dbContext.SaveChangesAsync();

                // Notify clients about worker status changes
                await jobUpdateHub.BroadcastWorkerStatusUpdates(staleWorkers.Select(w => w.Id).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking worker heartbeats");
            }
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Job Queue Service stopping");
            _queueProcessorTimer?.Change(Timeout.Infinite, 0);
            _heartbeatTimer?.Change(Timeout.Infinite, 0);
            return base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _queueProcessorTimer?.Dispose();
            _heartbeatTimer?.Dispose();
            base.Dispose();
        }
    }
}
