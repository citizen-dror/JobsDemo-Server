using JobsServer.Domain.Interfaces.Services;
using JobsServer.Domain.Entities;
using System.Collections.Concurrent;

namespace JobQueueSystem.WorkerNodes.Services
{
    /// <summary>
    /// Main job processor that executes different types of jobs based on JobType 
    /// the core execution engine within each worker node.
    /// 
    /// responsible for:
    /// Taking a job that was assigned to a worker, Executing the job based on its type
    /// Reporting progress back to the caller , Handling different types of jobs with different processing logic
    /// Returning success/failure results
    /// </summary>
    public class JobProcessor : IJobProcessor
    {
       
        private readonly ILogger<JobProcessor> _logger;

        public JobProcessor(ILogger<JobProcessor> logger)
        {
            _logger = logger;
        }

        public async Task<JobProcessResult> ProcessJob(Job job, IProgress<int> progress, CancellationToken token)
        {
            _logger.LogInformation($"Processing job {job.Id} ({job.JobName}) of type {job.JobType}");
            try
            {
                return job.JobType switch
                {
                    "DataProcessing" => await ProcessDataJob(job, progress, token),
                    "FileConversion" => await ProcessFileJob(job, progress, token),
                    "Notification" => await ProcessNotificationJob(job, progress, token),
                    "Report" => await ProcessReportJob(job, progress, token),
                    _ => await ProcessGenericJob(job, progress, token)
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning($"Job {job.Id} was cancelled.");
                return new JobProcessResult
                {
                    Success = false,
                    ErrorMessage = "Job was cancelled by user."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing job {job.Id}");
                return new JobProcessResult
                {
                    Success = false,
                    ErrorMessage = $"Job processor exception: {ex.Message}"
                };
            }            
        }

        private async Task<JobProcessResult> ProcessDataJob(Job job, IProgress<int> progress, CancellationToken token)
        {
            _logger.LogInformation($"Processing data job {job.Id}");

            for (int i = 0; i <= 100; i += 10)
            {
                token.ThrowIfCancellationRequested();
                if (i > 0) await Task.Delay(5000, token);
                progress.Report(i);
            }

            return new JobProcessResult
            {
                Success = true,
                ResultData = new { ProcessedRecords = 150 }
            };
        }

        private async Task<JobProcessResult> ProcessFileJob(Job job, IProgress<int> progress, CancellationToken token)
        {
            _logger.LogInformation($"Processing file job {job.Id}");

            // Simulate file processing with progress
            for (int i = 0; i <= 100; i += 5)
            {
                token.ThrowIfCancellationRequested();
                if (i > 0)
                {
                    await Task.Delay(1000); // Simulate work being done
                }
                progress.Report(i);
            }

            // Here you would parse job.JobData to get file info and perform conversion

            return new JobProcessResult
            {
                Success = true,
                ResultData = new { ConvertedFilePath = "/path/to/converted/file.pdf" }
            };
        }

        private async Task<JobProcessResult> ProcessNotificationJob(Job job, IProgress<int> progress, CancellationToken token)
        {
            _logger.LogInformation($"Processing notification job {job.Id}");

            // Notifications typically have a simpler progress pattern
            // Simulate file processing with progress
            for (int i = 0; i <= 100; i += 5)
            {
                token.ThrowIfCancellationRequested();
                if (i > 0)
                {
                    await Task.Delay(1000); // Simulate work being done
                }
                progress.Report(i);
            }

            // Here you would parse job.JobData and send the notification

            return new JobProcessResult
            {
                Success = true,
                ResultData = new { NotificationSent = true, RecipientCount = 5 }
            };
        }

        private async Task<JobProcessResult> ProcessReportJob(Job job, IProgress<int> progress, CancellationToken token)
        {
            _logger.LogInformation($"Processing report job {job.Id}");

            // Simulate report generation with uneven progress
            int[] steps = { 10, 20, 40, 50, 60, 85, 95, 100 };

            foreach (var step in steps)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(3000); // Reports often have longer processing times
                progress.Report(step);
            }

            // Here you would parse job.JobData and generate the report

            return new JobProcessResult
            {
                Success = true,
                ResultData = new { ReportUrl = "/reports/generated/report_12345.xlsx" }
            };
        }

        private async Task<JobProcessResult> ProcessGenericJob(Job job, IProgress<int> progress, CancellationToken token)
        {
            _logger.LogWarning($"Processing unknown job type: {job.JobType}");

            // Simple linear progress for unknown job types
            for (int i = 0; i <= 100; i += 20)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(200);
                progress.Report(i);
            }

            return new JobProcessResult
            {
                Success = true,
                ResultData = new { Message = "Generic job completed" }
            };
        }
    }
}
