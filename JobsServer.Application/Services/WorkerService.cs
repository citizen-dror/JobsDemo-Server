using JobsServer.Domain.DTOs;
using JobsServer.Domain.Entities;
using JobsServer.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Application.Services
{
    public interface IWorkerService
    {
        Task<IEnumerable<WorkerNode>> GetWorkersAsync(WorkerStatus? status);
        Task<WorkerNode> RegisterWorkerAsync(WorkerNode worker);
        Task<bool> ProcessHeartbeatAsync(string id, WorkerHeartbeatDto heartbeat);
    }

    public class WorkerService : IWorkerService
    {
        private readonly IWorkerRepository _workerRepository;
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(IWorkerRepository workerRepository, ILogger<WorkerService> logger)
        {
            _workerRepository = workerRepository;
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

        public async Task<bool> ProcessHeartbeatAsync(string id, WorkerHeartbeatDto heartbeat)
        {
            var updated = await _workerRepository.UpdateWorkerHeartbeatAsync(id, heartbeat);
            return updated;
        }
    }
}
