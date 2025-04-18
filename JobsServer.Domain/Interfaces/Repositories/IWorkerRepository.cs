using JobQueueSystem.Core.DTOs;
using JobQueueSystem.Core.Enums;
using JobsServer.Domain.Entities;

namespace JobsServer.Domain.Interfaces.Repositories
{
    public interface IWorkerRepository
    {
        Task<IEnumerable<WorkerNode>> GetWorkersByStatusAsync(WorkerStatus? status);
        Task<WorkerNode> GetWorkerByNameAsync(string name);
        Task<WorkerNode?> GetWorkerByIdAsync(string id);
        Task<List<Job>> GetWorkerJobsAsync(string workerId);
        Task AddAsync(WorkerNode worker);
        Task SaveAsync();
        Task<bool> UpdateWorkerHeartbeatAsync(string id, WorkerHeartbeatDto heartbeat);
    }
}
