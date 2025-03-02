using JobsServer.Domain.Enums;

namespace JobsServer.Domain.Entities
{
    public class Job
    {
        public int Id { get; set; }
        public string JobName { get; set; } = string.Empty;
        public JobPriority Priority { get; set; } = JobPriority.Regular;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Progress { get; set; } = 0;
    }
}
