using JobsServer.Domain.DTOs;
using JobsServer.Domain.Entities;
using JobsServer.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Application.Interfaces
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
