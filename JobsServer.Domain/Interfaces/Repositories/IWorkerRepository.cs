using JobsServer.Domain.DTOs;
using JobsServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
