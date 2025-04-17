using JobQueueSystem.Core.Data;
using JobQueueSystem.Core.Interfaces;
using JobsServer.Domain.Entities;
using JobsServer.Domain.Enums;

namespace JobQueueSystem.QueueService.Services
{
    /// <summary>
    /// Distribute Jobs to Workers by Workers capacity and jobs priority
    /// </summary>
    public class JobDistributor
    {
        private readonly ILogger<JobDistributor> _logger;
        private readonly JobDbContext _dbContext;
        private readonly IWorkerApiClient _workerApiClient;

        public JobDistributor(
            ILogger<JobDistributor> logger,
            JobDbContext dbContext,
            IWorkerApiClient workerApiClient
            )
        {
            _logger = logger;
            _dbContext = dbContext;
            _workerApiClient = workerApiClient;
        }

        public async Task DistributeJobs(List<Job> pendingJobs, List<WorkerNode> availableWorkers)
        {
            if (!pendingJobs.Any() || !availableWorkers.Any())
                return;

            var workersByCapacity = GroupWorkersByAvailableCapacity(availableWorkers);
            var prioritizedJobs = GetPrioritizedJobs(pendingJobs);

            foreach (var job in prioritizedJobs)
            {
                var selectedWorker = SelectWorkerWithCapacity(workersByCapacity);
                if (selectedWorker == null)
                {
                    _logger.LogInformation($"No workers available with capacity for job {job.Id}");
                    break;
                }

                PrepareJobAssignment(job, selectedWorker);

                _logger.LogInformation($"Assigning job {job.Id} ({job.JobName}) to worker {selectedWorker.Name}");

                var assignmentSuccessful = await _workerApiClient.AssignJobToWorker(selectedWorker.Id, job);
                if (!assignmentSuccessful)
                {
                    HandleAssignmentFailure(job, selectedWorker);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private Dictionary<int, List<WorkerNode>> GroupWorkersByAvailableCapacity(List<WorkerNode> workers)
        {
            return workers
                .OrderBy(w => w.ActiveJobCount)
                .GroupBy(w => w.ConcurrencyLimit - w.ActiveJobCount)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private List<Job> GetPrioritizedJobs(List<Job> jobs)
        {
            return jobs
                .OrderByDescending(j => j.Priority)
                .ThenBy(j => j.ScheduledTime ?? DateTime.MinValue)
                .ToList();
        }

        private WorkerNode SelectWorkerWithCapacity(Dictionary<int, List<WorkerNode>> workersByCapacity)
        {
            foreach (var capacity in workersByCapacity.Keys.OrderByDescending(k => k))
            {
                if (workersByCapacity[capacity].Any())
                {
                    var worker = workersByCapacity[capacity].First();
                    workersByCapacity[capacity].Remove(worker);

                    var newCapacity = capacity - 1;
                    if (newCapacity > 0)
                    {
                        if (!workersByCapacity.ContainsKey(newCapacity))
                            workersByCapacity[newCapacity] = new List<WorkerNode>();
                        workersByCapacity[newCapacity].Add(worker);
                    }

                    return worker;
                }
            }

            return null;
        }

        private void PrepareJobAssignment(Job job, WorkerNode worker)
        {
            job.Status = JobStatus.InProgress;
            job.AssignedWorker = worker.Id;
            job.StartTime = DateTime.UtcNow;

            worker.Status = WorkerStatus.Busy;
            worker.ActiveJobCount++;
            worker.CurrentJobId = job.Id;
        }

        private void HandleAssignmentFailure(Job job, WorkerNode worker)
        {
            _logger.LogWarning($"Failed to assign job {job.Id} to worker {worker.Name}. Will try again later.");

            job.Status = JobStatus.Pending;
            job.AssignedWorker = null;
            job.StartTime = null;

            worker.ActiveJobCount--;
            worker.Status = worker.ActiveJobCount > 0 ? WorkerStatus.Busy : WorkerStatus.Idle;
        }
        
    }
}
