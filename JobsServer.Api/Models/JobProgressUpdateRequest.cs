namespace JobsServer.Api.Models
{
    public class JobProgressUpdateRequest
    {
        public int JobId { get; set; }
        public int Progress { get; set; }
    }
}
