using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using JobQueueSystem.Core.Enums;
using JobsServer.Domain.Entities;
using JobsServer.Domain.Interfaces.APIs;

namespace JobQueueSystem.WorkerNodes.Services
{
    //communication between the worker nodes and the job queue service
    public class WorkerApiClient : IWorkerApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WorkerApiClient> _logger;
        private readonly WorkerSettings _settings;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public WorkerApiClient(
            IHttpClientFactory httpClientFactory,
            ILogger<WorkerApiClient> logger,
            IOptions<WorkerSettings> settings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _settings = settings.Value;

            // Configure the base address from settings
            _httpClient.BaseAddress = new Uri(_settings.QueueServiceUrl);
        }

        public async Task<WorkerNode> RegisterWorker(WorkerNode worker, int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/worker/register", worker, _jsonOptions);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadFromJsonAsync<WorkerNode>(_jsonOptions);
                    }

                    _logger.LogWarning($"Attempt {attempt}: Failed to register worker. Status: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Attempt {attempt}: Exception occurred while registering worker");
                }

                if (attempt < maxRetries)
                {
                    var delaySeconds = 2 * attempt; // exponential backoff: 2s, 4s...
                    _logger.LogInformation($"Waiting {delaySeconds} seconds before retrying...");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            _logger.LogError("All attempts to register the worker have failed.");
            return null;
        }

        public async Task SendHeartbeat(string workerId, WorkerStatus status, int activeJobCount)
        {
            try
            {
                var heartbeat = new
                {
                    WorkerId = workerId,
                    Status = status,
                    ActiveJobCount = activeJobCount,
                    Timestamp = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync("api/worker/heartbeat", heartbeat, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to send heartbeat. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending heartbeat");
            }
        }

        public async Task UpdateWorkerStatus(string workerId, WorkerStatus status)
        {
            try
            {
                var statusUpdate = new
                {
                    Status = status
                };

                var response = await _httpClient.PutAsJsonAsync($"api/worker/{workerId}/status", statusUpdate, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to update worker status. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating worker status");
            }
        }

        public async Task<bool> AssignJobToWorker(string workerId, Job job)
        {
            try
            {
                // This method is typically called by the queue service, not by workers
                // But it's included for completeness
                var response = await _httpClient.PostAsJsonAsync($"api/worker/{workerId}/jobs", job, _jsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while assigning job {job.Id} to worker");
                return false;
            }
        }

        public async Task UpdateJobStatus(Job job)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/worker/{job.Id}/jobstatus", job.Status, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to update job status. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while updating status for job {job.Id}");
            }
        }

        public async Task UpdateJobProgress(int jobId, int progress)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(
                    $"api/worker/{jobId}/progress",
                    progress,
                    _jsonOptions
                );

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to update job progress for JobId={jobId}. Status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while updating progress for job {jobId}");
            }
        }

        public async Task<Job> GetJob(int jobId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Job>($"api/jobs/{jobId}", _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred while getting job {jobId}");
                return null;
            }
        }

        public async Task<Job[]> GetPendingJobsForWorker(string workerId, int maxJobs)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Job[]>($"api/workers/{workerId}/pendingjobs?maxJobs={maxJobs}", _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting pending jobs");
                return Array.Empty<Job>();
            }
        }
    }
}
