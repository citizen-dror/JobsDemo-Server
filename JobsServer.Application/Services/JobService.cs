using AutoMapper;
using JobQueueSystem.Core.DTOs;
using JobQueueSystem.Core.Enums;
using JobsServer.Domain.Entities;
using JobsServer.Domain.Interfaces.Services;
using JobsServer.Domain.Interfaces.Repositories;
using JobsServer.Domain.Interfaces.APIs;

namespace JobsServer.Application.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _repository;
        private readonly IMapper _mapper;
        private readonly IJobUpdateNotifier _jobUpdateNotifier;

        public JobService(IJobRepository repository, IMapper mapper, IJobUpdateNotifier jobUpdateNotifier)
        {
            _repository = repository;
            _mapper = mapper;
            _jobUpdateNotifier = jobUpdateNotifier;
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
            job.CreatedTime = DateTime.UtcNow;
            await _repository.AddAsync(job);
            return _mapper.Map<JobDto>(job);
        }

        public async Task<bool> UpdateJobProgress(int id, int progress)
        {
            var job = await _repository.GetByIdAsync(id);
            if (job != null)
            {
                job.Progress = progress;
                job.Status = progress == 100 ? JobStatus.Completed : JobStatus.InProgress;
                await _repository.UpdateAsync(job);
                // Notify API via JobUpdateNotifier (which will trigger SignalR)
                await _jobUpdateNotifier.NotifyJobUpdate(job);
                return true;
            }
            return false;
        }

        public async Task<bool> StopJobAsync(int id)
        {
            var job = await _repository.GetByIdAsync(id);
            if (job == null || job.Status != JobStatus.InProgress) return false;
            job.Status = JobStatus.Failed;
            await _repository.UpdateAsync(job);
            return true;
        }

        public async Task<bool> RestartJobAsync(int id)
        {
            var job = await _repository.GetByIdAsync(id);
            if (job == null || job.Status == JobStatus.InProgress) return false;
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
