using JobsServer.Domain.Entities;

namespace JobsServer.Application.Notifications
{
    public interface IJobUpdateNotifier
    {
        Task NotifyJobUpdate(Job job);
    }
}
