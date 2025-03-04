using JobsServer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Infrastructure.Repositories
{
    public interface IWorkerRepository
    {
        Task<IEnumerable<WorkerNode>> GetWorkersByStatusAsync(WorkerStatus? status);
        Task<WorkerNode> GetWorkerByNameAsync(string name);
        Task AddAsync(WorkerNode worker);
        Task SaveAsync();
    }

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
