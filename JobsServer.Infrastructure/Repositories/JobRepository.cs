using JobsServer.Domain.Entities;
using JobsServer.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;


namespace JobsServer.Infrastructure.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _context;

        public JobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Job>> GetAllAsync() => await _context.Jobs.ToListAsync();

        public async Task<Job?> GetByIdAsync(int id) => await _context.Jobs.FindAsync(id);

        public async Task AddAsync(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Job job)
        {
            _context.Jobs.Update(job);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Job job)
        {
            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
        }
    }
}
