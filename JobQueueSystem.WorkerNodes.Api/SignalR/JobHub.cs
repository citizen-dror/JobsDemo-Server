using JobsServer.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace JobQueueSystem.WorkerNodes.Api.SignalR
{
    public class JobHub : Hub
    {
        public async Task SendJobUpdate(Job job)
        {
            await Clients.All.SendAsync("ReceiveJobUpdate", job);
        }
    }
}
