using Microsoft.EntityFrameworkCore;
using JobQueueSystem.Core.DTOs;
using JobQueueSystem.Core.Enums;
using JobsServer.Domain.Entities;
using JobsServer.Domain.Interfaces.Repositories;

namespace JobsServer.Infrastructure.Repositories
{

    public class WorkerRepository : IWorkerRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public WorkerRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<WorkerNode>> GetWorkersByStatusAsync(WorkerStatus? status)
        {
            IQueryable<WorkerNode> query = _dbContext.WorkerNodes;

            if (status.HasValue)
            {
                query = query.Where(w => w.Status == status.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<WorkerNode> GetWorkerByNameAsync(string name)
        {
            return await _dbContext.WorkerNodes
                .FirstOrDefaultAsync(w => w.Name == name && w.Status != WorkerStatus.Offline);
        }

        public async Task<WorkerNode?> GetWorkerByIdAsync(string id)
        {
            return await _dbContext.WorkerNodes.FindAsync(id);
        }

        public async Task<List<Job>> GetWorkerJobsAsync(string workerId)
        {
            return await _dbContext.Jobs
                .Where(j => j.AssignedWorker == workerId && j.Status == JobStatus.InProgress)
                .ToListAsync();
        }

        public async Task<bool> UpdateWorkerHeartbeatAsync(string id, WorkerHeartbeatDto heartbeat)
        {
            var worker = await _dbContext.WorkerNodes.FindAsync(id);
            if (worker == null)
                return false;

            var statusChanged = worker.Status != heartbeat.Status;

            worker.LastHeartbeat = heartbeat.Timestamp;
            worker.Status = heartbeat.Status;
            worker.ActiveJobCount = heartbeat.ActiveJobCount;

            await _dbContext.SaveChangesAsync();

            //if (statusChanged)
            //{
            //    await _jobUpdateHub.BroadcastWorkerStatusUpdates(new List<string> { worker.Id });
            //}

            return true;
        }

        public async Task AddAsync(WorkerNode worker)
        {
            await _dbContext.WorkerNodes.AddAsync(worker);
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
