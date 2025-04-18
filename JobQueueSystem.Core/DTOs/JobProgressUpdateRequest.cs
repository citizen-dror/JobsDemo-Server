namespace JobQueueSystem.Core.DTOs
{
    public class JobProgressUpdateRequest
    {
        public int JobId { get; set; }
        public int Progress { get; set; }
    }
}
