using JobQueueSystem.Core.DTOs;
using JobsServer.Domain.Interfaces.APIs;
using Microsoft.AspNetCore.SignalR;

namespace JobsServer.Api.SignalR
{
    public class JobUpdateNotifier : IJobUpdateNotifier
    {
        private readonly IHubContext<JobHub> _hubContext;

        public JobUpdateNotifier(IHubContext<JobHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyJobUpdate(JobStatusDto jobStatus)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveJobUpdate", jobStatus);
        }

    }
}
