using JobsServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Infrastructure.Repositories
{
    public interface IJobRepository
    {
        Task<IEnumerable<Job>> GetAllAsync();
        Task<Job?> GetByIdAsync(int id);
        Task AddAsync(Job job);
        Task UpdateAsync(Job job);
        Task DeleteAsync(Job job);
    }
}
