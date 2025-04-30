using JobQueueSystem.Core.DTOs;

namespace JobsServer.Domain.Interfaces.Services
{
    public interface IJobNotificationForwarderService
    {
        Task NotifyProgressAsync(JobStatusDto jobStatus);
    }
}
