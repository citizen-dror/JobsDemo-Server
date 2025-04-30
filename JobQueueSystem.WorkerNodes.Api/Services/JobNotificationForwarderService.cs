using JobQueueSystem.Core.DTOs;
using JobsServer.Domain.Interfaces.Services;

namespace JobQueueSystem.WorkerNodes.Api.Services
{
    public class JobNotificationForwarderService : IJobNotificationForwarderService
    {
        // private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _client;

        public JobNotificationForwarderService(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("JobsServerClient");
        }

        public async Task NotifyProgressAsync(JobStatusDto jobStatus)
        {
            var res = await _client.PostAsJsonAsync("internal/notifications/updateProgress", jobStatus);
            if (!res.IsSuccessStatusCode)
            {
                var msg = await res.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to notify progress: {msg}");
            }
        }

    }
}