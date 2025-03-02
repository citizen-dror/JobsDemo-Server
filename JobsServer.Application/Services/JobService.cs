using AutoMapper;
using JobsServer.Application.DTOs;
using JobsServer.Domain.Entities;
using JobsServer.Domain.Enums;
using JobsServer.Infrastructure.Repositories;

namespace JobsServer.Application.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _repository;
        private readonly IMapper _mapper;

        public JobService(IJobRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<JobDto>> GetAllJobsAsync()
        {
            var jobs = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<JobDto>>(jobs);
        }

        public async Task<JobDto?> GetJobByIdAsync(int id)
        {
            var job = await _repository.GetByIdAsync(id);
            return job == null ? null : _mapper.Map<JobDto>(job);
        }

        public async Task<JobDto> CreateJobAsync(CreateJobDto createJobDto)
        {
            var job = _mapper.Map<Job>(createJobDto);
            job.Status = JobStatus.Pending;
            await _repository.AddAsync(job);
            return _mapper.Map<JobDto>(job);
        }

        public async Task<bool> StopJobAsync(int id)
        {
            var job = await _repository.GetByIdAsync(id);
            if (job == null || job.Status != JobStatus.Running) return false;
            job.Status = JobStatus.Failed;
            await _repository.UpdateAsync(job);
            return true;
        }

        public async Task<bool> RestartJobAsync(int id)
        {
            var job = await _repository.GetByIdAsync(id);
            if (job == null || job.Status == JobStatus.Running) return false;
            job.Status = JobStatus.Pending;
            job.Progress = 0;
            await _repository.UpdateAsync(job);
            return true;
        }

        public async Task<bool> DeleteCompletedOrFailedJobAsync(int id)
        {
            var job = await _repository.GetByIdAsync(id);
            if (job == null || (job.Status != JobStatus.Completed && job.Status != JobStatus.Failed)) return false;
            await _repository.DeleteAsync(job);
            return true;
        }
    }
}
