﻿using JobQueueSystem.Core.Interfaces;
using JobsServer.Domain.Entities;
using JobsServer.Domain.Enums;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace JobQueueSystem.WorkerNodes.Services
{
    public class WorkerNodeService : BackgroundService
    {
        private readonly ILogger<WorkerNodeService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IJobProcessor _jobProcessor;
        private readonly WorkerSettings _settings;
        private readonly ConcurrentDictionary<int, Job> _activeJobs = new ConcurrentDictionary<int, Job>();
        private Timer _heartbeatTimer;
        private string _workerId;
        private WorkerStatus _status = WorkerStatus.Idle;

        public WorkerNodeService(
           ILogger<WorkerNodeService> logger,
           IServiceScopeFactory scopeFactory,
           IJobProcessor jobProcessor,
           IOptions<WorkerSettings> settings)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _jobProcessor = jobProcessor;
            _settings = settings.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Worker Node Service starting with name {_settings.WorkerName}");

            // Create a scope to resolve scoped services
            using (var scope = _scopeFactory.CreateScope())
            {
                var apiClient = scope.ServiceProvider.GetRequiredService<IWorkerApiClient>();
                // Register worker with job queue service
                var worker = new JobsServer.Domain.Entities.WorkerNode
                {
                    Name = _settings.WorkerName,
                    ConcurrencyLimit = _settings.ConcurrencyLimit,
                    Status = WorkerStatus.Idle
                };
                var registration = await apiClient.RegisterWorker(worker);
                if (registration == null)
                {
                    _logger.LogError("Failed to register worker with job queue service");
                    return;
                }
                _workerId = registration.Id;
                _logger.LogInformation($"Worker registered with ID: {_workerId}");
            }

            // Start heartbeat timer
            _heartbeatTimer = new Timer(SendHeartbeat, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        private async void SendHeartbeat(object state)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var apiClient = scope.ServiceProvider.GetRequiredService<IWorkerApiClient>();
                    await apiClient.SendHeartbeat(_workerId, _status, _activeJobs.Count);
                    _logger.LogDebug($"Heartbeat sent: Status={_status}, ActiveJobs={_activeJobs.Count}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending heartbeat");
            }
        }

        // This method would be called by the API controller when jobs are assigned
        public async Task<bool> AcceptJob(Job job)
        {
            if (_activeJobs.Count >= _settings.ConcurrencyLimit)
            {
                _logger.LogWarning($"Rejecting job {job.Id}: Worker at capacity");
                return false;
            }

            if (!_activeJobs.TryAdd(job.Id, job))
            {
                _logger.LogWarning($"Job {job.Id} is already being processed by this worker");
                return false;
            }

            UpdateWorkerStatus();
            // Process job asynchronously
            StartBackgroundJobExecution(job);
            return true;
        }

        private async Task ProcessJob(Job job)
        {
            _logger.LogInformation($"Starting job {job.Id} ({job.JobName})");

            job.Status = JobStatus.InProgress;
            job.Progress = 0;
            using (var scope = _scopeFactory.CreateScope())
            {
                var apiClient = scope.ServiceProvider.GetRequiredService<IWorkerApiClient>();
                // Notify job queue service that we've started
                await apiClient.UpdateJobStatus(job);

                try
                {
                    // Process the job using the job processor
                    var result = await _jobProcessor.ProcessJob(job, new Progress<int>(async progress =>
                    {
                        // Update progress
                        job.Progress = progress;
                        await apiClient.UpdateJobProgress(job.Id, progress);
                    }));

                    // Update job status based on result
                    job.Status = result.Success ? JobStatus.Completed : JobStatus.Failed;
                    job.ErrorMessage = result.ErrorMessage;
                    job.EndTime = DateTime.UtcNow;

                    _logger.LogInformation($"Completed job {job.Id} with status {job.Status}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception processing job {job.Id}");
                    job.Status = job.RetryCount < job.MaxRetries ? JobStatus.Retrying : JobStatus.Failed;
                    job.ErrorMessage = ex.Message;
                    job.EndTime = DateTime.UtcNow;
                }

                // Notify job queue service of completion
                await apiClient.UpdateJobStatus(job);
            }
        }

        private void StartBackgroundJobExecution(Job job)
        {
            _ = Task.Run(() => ExecuteJobInternalAsync(job));
        }
        private async Task ExecuteJobInternalAsync(Job job)
        {
            try
            {
                await ProcessJob(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing job {job.Id}");
                await HandleJobFailure(job, ex);
            }
            finally
            {
                _activeJobs.TryRemove(job.Id, out _);
                UpdateWorkerStatus();
            }
        }
        private async Task HandleJobFailure(Job job, Exception ex)
        {
            job.Status = job.RetryCount < job.MaxRetries ? JobStatus.Retrying : JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.EndTime = DateTime.UtcNow;

            using var scope = _scopeFactory.CreateScope();
            var apiClient = scope.ServiceProvider.GetRequiredService<IWorkerApiClient>();
            await apiClient.UpdateJobStatus(job);
        }

        private void UpdateWorkerStatus()
        {
            var newStatus = _activeJobs.Count > 0 ? WorkerStatus.Busy : WorkerStatus.Idle;
            if (newStatus != _status)
            {
                _status = newStatus;
                _logger.LogInformation($"Worker status changed to {_status}");
            }
        }

        public override Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker Node Service stopping");
            _heartbeatTimer?.Change(Timeout.Infinite, 0);

            // Notify job queue service that we're going offline
            if (!string.IsNullOrEmpty(_workerId))
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var apiClient = scope.ServiceProvider.GetRequiredService<IWorkerApiClient>();
                        apiClient.UpdateWorkerStatus(_workerId, WorkerStatus.Offline).Wait();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to notify job queue service about worker going offline");
                }
            }

            return base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _heartbeatTimer?.Dispose();
            base.Dispose();
        }
    }
}
