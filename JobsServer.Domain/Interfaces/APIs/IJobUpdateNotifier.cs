using JobQueueSystem.Core.DTOs;

namespace JobsServer.Domain.Interfaces.APIs
{
    public interface IJobUpdateNotifier
    {
        Task NotifyJobUpdate(JobStatusDto jobStatus);
    }
}
