using JobQueueSystem.Core.DTOs;

namespace JobsServer.Domain.Interfaces.Services
{
    public interface IJobService
    {
        Task<IEnumerable<JobDto>> GetAllJobsAsync();
        Task<JobDto?> GetJobByIdAsync(int id);
        Task<JobDto> CreateJobAsync(CreateJobDto createJobDto);
        Task<bool> UpdateJobProgress(int id, int progress);
        Task<bool> StopJobAsync(int id);
        Task<bool> RestartJobAsync(int id);
        Task<bool> DeleteCompletedOrFailedJobAsync(int id);
    }
}
