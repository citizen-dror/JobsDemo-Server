using JobsServer.Domain.Entities;
using JobsServer.Domain.Interfaces.APIs;
using Microsoft.AspNetCore.SignalR;

namespace JobQueueSystem.WorkerNodes.Api.SignalR
{
    public class JobUpdateNotifier : IJobUpdateNotifier
    {
        private readonly IHubContext<JobHub> _hubContext;

        public JobUpdateNotifier(IHubContext<JobHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyJobUpdate(Job job)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveJobUpdate", job);
        }
    }
}
