using JobQueueSystem.Core.Enums;

namespace JobQueueSystem.Core.DTOs
{
    public class WorkerHeartbeatDto
    {
        public string WorkerId { get; set; }
        public WorkerStatus Status { get; set; }
        public int ActiveJobCount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
