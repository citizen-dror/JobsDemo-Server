using JobsServer.Domain.Entities;

namespace JobsServer.Domain.Interfaces.Services
{
    /// <summary>
    /// Interface defining the job processor operations
    /// </summary>
    public interface IJobProcessor
    {
        Task<JobProcessResult> ProcessJob(Job job, IProgress<int> progress, CancellationToken token);       
    }

    /// <summary>
    /// Result of job processing
    /// </summary>
    public class JobProcessResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public object? ResultData { get; set; }
    }
}
