using JobsServer.Domain.Entities;

namespace JobsServer.Domain.Interfaces.APIs
{
    public interface IJobUpdateNotifier
    {
        Task NotifyJobUpdate(Job job);
    }
}
