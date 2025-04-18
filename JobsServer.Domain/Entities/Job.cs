using JobQueueSystem.Core.Enums;

namespace JobsServer.Domain.Entities
{
    public class Job
    {
        public int Id { get; set; }
        public string JobName { get; set; } = string.Empty;
        public JobPriority Priority { get; set; } = JobPriority.Regular;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public DateTime? ScheduledTime { get; set; } // For delayed execution
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Progress { get; set; } = 0;
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
        public string? AssignedWorker { get; set; }
        public string? JobData { get; set; } // Serialized job parameters
        public string JobType { get; set; } = string.Empty; // Type of job for worker to process
        public DateTime CreatedTime { get; set; }
    }
}
