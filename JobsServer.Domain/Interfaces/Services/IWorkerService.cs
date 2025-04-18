using JobQueueSystem.Core.DTOs;
using JobQueueSystem.Core.Enums;
using JobsServer.Domain.Entities;

namespace JobsServer.Domain.Interfaces.Services
{
    public interface IWorkerService
    {
        Task<IEnumerable<WorkerNode>> GetWorkersAsync(WorkerStatus? status);
        Task<WorkerNode> RegisterWorkerAsync(WorkerNode worker);
        Task<bool> ProcessHeartbeatAsync(string workerId, WorkerHeartbeatDto heartbeat);
        Task<bool> UpdateWorkerStatusAsync(string id, WorkerStatus newStatus);
        Task<AssignJobResult> AssignJobToWorkerAsync(string workerId, Job job);
        Task<List<Job>?> GetWorkerJobsAsync(string workerId);
    }
}
