using JobQueueSystem.Core.Enums;

namespace JobsServer.Domain.Entities
{
    public class WorkerNode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public WorkerStatus Status { get; set; } = WorkerStatus.Idle;
        public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
        public int CurrentJobId { get; set; }
        public int ConcurrencyLimit { get; set; } = 1;
        public int ActiveJobCount { get; set; } = 0;
    }


}
