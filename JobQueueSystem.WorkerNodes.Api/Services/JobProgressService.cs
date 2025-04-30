using AutoMapper;
using JobQueueSystem.Core.DTOs;
using JobQueueSystem.Core.Enums;
using JobsServer.Domain.Interfaces.Repositories;
using JobsServer.Domain.Interfaces.Services;

namespace JobQueueSystem.WorkerNodes.Api.Services
{
    public class JobProgressService: IJobProgressService
    {
        private readonly IJobRepository _repository;
        // private readonly IMapper _mapper;
        // private readonly IJobUpdateNotifier _jobUpdateNotifier;

        public JobProgressService(IJobRepository repository)
        {
            _repository = repository;
            // _mapper = mapper;
            //_jobUpdateNotifier = jobUpdateNotifier;
        }
        public async Task<bool> UpdateJobStatus(int id, JobStatus status)
        {
            var job = await _repository.GetByIdAsync(id);
            if (job != null)
            {
                job.Status = status;
                await _repository.UpdateAsync(job);
                // Notify API via JobUpdateNotifier (which will trigger SignalR)
                // await _jobUpdateNotifier.NotifyJobUpdate(job);
                return true;
            }
            return false;
        }
        public async Task<JobStatusDto?> UpdateJobProgress(int id, int progress)
        {
            var job = await _repository.GetByIdAsync(id);
            if (job != null)
            {
                job.Progress = progress;
                job.Status = progress == 100 ? JobStatus.Completed : JobStatus.InProgress;

                await _repository.UpdateAsync(job);

                return new JobStatusDto
                {
                    Id = job.Id,
                    Progress = job.Progress,
                    Status = job.Status
                };
            }

            return null;
        }

    }
}
