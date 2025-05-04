
using JobQueueSystem.Core.DTOs;
using JobQueueSystem.Core.Enums;
using JobsServer.Domain.DTOs;
using JobsServer.Domain.Entities;
using JobsServer.Domain.Interfaces.APIs;
using JobsServer.Domain.Interfaces.Repositories;
using JobsServer.Domain.Interfaces.Services;
using JobsServer.Infrastructure.RabbitMQ;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JobQueueSystem.QueueService.Services
{
 

    public class WorkerService : IWorkerService
    {
        private readonly IWorkerRepository _workerRepository;
        private readonly IRabbitSender _rabbitSender;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(IWorkerRepository workerRepository, IRabbitSender rabbitSender, ILogger<WorkerService> logger)
        {
            _workerRepository = workerRepository;
            _rabbitSender = rabbitSender;
            _logger = logger;
        }
        public async Task<IEnumerable<WorkerNode>> GetWorkersAsync(WorkerStatus? status)
        {
            return await _workerRepository.GetWorkersByStatusAsync(status);
        }

        public async Task<WorkerNode> RegisterWorkerAsync(WorkerNode worker)
        {
            var existingWorker = await _workerRepository.GetWorkerByNameAsync(worker.Name);

            if (existingWorker != null)
            {
                if (existingWorker.Status == WorkerStatus.Offline)
                {
                    existingWorker.Status = WorkerStatus.Idle;
                    existingWorker.LastHeartbeat = DateTime.UtcNow;
                    existingWorker.ActiveJobCount = 0;
                    await _workerRepository.SaveAsync();

                    _logger.LogInformation($"Reactivated worker {existingWorker.Name} ({existingWorker.Id})");

                    return existingWorker;
                }

                return null; // Return null to indicate a conflict (worker already registered and active)
            }

            if (string.IsNullOrEmpty(worker.Id))
            {
                worker.Id = Guid.NewGuid().ToString();
            }

            worker.LastHeartbeat = DateTime.UtcNow;
            worker.Status = WorkerStatus.Idle;

            await _workerRepository.AddAsync(worker);
            await _workerRepository.SaveAsync();

            _logger.LogInformation($"Registered new worker {worker.Name} ({worker.Id})");

            return worker;
        }

        public async Task<bool> ProcessHeartbeatAsync(string workerId, WorkerHeartbeatDto heartbeat)
        {
            var updated = await _workerRepository.UpdateWorkerHeartbeatAsync(workerId, heartbeat);
            return updated;
        }

        public async Task<bool> UpdateWorkerStatusAsync(string id, WorkerStatus newStatus)
        {
            var worker = await _workerRepository.GetWorkerByIdAsync(id);
            if (worker == null)
                return false;

            worker.Status = newStatus;
            worker.LastHeartbeat = DateTime.UtcNow;

            await _workerRepository.SaveAsync();
            //await _jobUpdateHub.BroadcastWorkerStatusUpdates(new List<string> { worker.Id });
            return true;
        }
        public async Task<AssignJobResult> AssignJobToWorkerAsync(string workerId, Job job)
        {
            var worker = await _workerRepository.GetWorkerByIdAsync(workerId);
            if (worker == null)
                return AssignJobResult.NotFound;

            if (worker.Status == WorkerStatus.Offline)
                return AssignJobResult.Offline;

            if (worker.ActiveJobCount >= worker.ConcurrencyLimit)
                return AssignJobResult.AtCapacity;

            // Publish job to worker's queue via RabbitMQ
            await PublishJobToWorker(workerId, job);
            _logger.LogInformation($"Assigning job {job.Id} to worker {worker.Name} ({worker.Id})");

            // Job processing logic happens elsewhere

            return AssignJobResult.Success;
        }
        public async Task PublishJobToWorker(string workerId, Job job)
        {
            try
            {
                var controlMessage = new JobControlMessage
                {
                    Type = JobControlType.AssignJobToWorker,
                    Job = job
                };
                var messageJson = JsonConvert.SerializeObject(controlMessage);
                var queueName = $"worker.{workerId}";
                await _rabbitSender.SendMessageAsync(queueName: queueName, message: messageJson, routingKeyOverride: queueName);

                _logger.LogInformation($"Published Assign job {job.Id} to worker {workerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing Assign job {job.Id} to worker {workerId}");
                throw;
            }
        }
        public async Task<List<Job>?> GetWorkerJobsAsync(string workerId)
        {
            var worker = await _workerRepository.GetWorkerByIdAsync(workerId);
            if (worker == null)
                return null;

            return await _workerRepository.GetWorkerJobsAsync(workerId);
        }
    }
}
