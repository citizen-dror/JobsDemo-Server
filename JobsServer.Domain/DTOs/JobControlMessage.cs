using JobQueueSystem.Core.Enums;
using JobsServer.Domain.Entities;

namespace JobsServer.Domain.DTOs
{
    public class JobControlMessage
    {
        public JobControlType Type { get; set; }
        public int JobId { get; set; }
        public Job? Job { get; set; }
    }
}
