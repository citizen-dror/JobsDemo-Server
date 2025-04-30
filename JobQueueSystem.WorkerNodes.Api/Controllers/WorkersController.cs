using JobQueueSystem.Core.DTOs;
using JobQueueSystem.Core.Enums;
using JobQueueSystem.WorkerNodes.Api.Services;
using JobsServer.Domain.Entities;
using JobsServer.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JobQueueSystem.WorkerNodes.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkerController : ControllerBase
    {
        private readonly IWorkerService _workerService;
        private readonly IJobProgressService _jobProgressService;
        private readonly IJobNotificationForwarderService _jobNotifier;
        private readonly ILogger<WorkerController> _logger;

        public WorkerController(IWorkerService workerService, IJobProgressService jobProgressService, IJobNotificationForwarderService jobNotificationForwarderService, ILogger<WorkerController> logger)
        {
            _workerService = workerService;
            _jobProgressService = jobProgressService;
            _jobNotifier = jobNotificationForwarderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkerNode>>> GetWorkers([FromQuery] WorkerStatus? status)
        {
            try
            {
                var workers = await _workerService.GetWorkersAsync(status);
                return Ok(workers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching workers.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<WorkerNode>> RegisterWorker(WorkerNode worker)
        {
            try
            {
                var registeredWorker = await _workerService.RegisterWorkerAsync(worker);
                return registeredWorker != null ? 
                    Ok(registeredWorker) : 
                    Conflict($"Worker with name '{worker.Name}' is already registered and active");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during worker registration.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("heartbeat")]
        public async Task<IActionResult> SendHeartbeat([FromBody] WorkerHeartbeatDto heartbeat)
        {
            var updated = await _workerService.ProcessHeartbeatAsync(heartbeat.WorkerId, heartbeat);
            if (!updated)
                return NotFound();

            return Ok();
        }

        [HttpPost("{id}/status")]
        public async Task<IActionResult> UpdateWorkerStatus(string id, [FromBody] WorkerStatusUpdateDto statusUpdate)
        {
            var success = await _workerService.UpdateWorkerStatusAsync(id, statusUpdate.Status);
            if (!success)
                return NotFound();

            return Ok();
        }

        [HttpPost("{id}/jobs")]
        public async Task<ActionResult<bool>> AssignJobToWorker(string id, [FromBody] Job job)
        {
            var result = await _workerService.AssignJobToWorkerAsync(id, job);

            return result switch
            {
                AssignJobResult.Success => Ok(true),
                AssignJobResult.NotFound => NotFound($"Worker with ID {id} not found"),
                AssignJobResult.Offline => BadRequest("Cannot assign job to offline worker"),
                AssignJobResult.AtCapacity => BadRequest("Worker is at capacity"),
                _ => StatusCode(500, "Unexpected error")
            };
        }

        [HttpGet("{id}/jobs")]
        public async Task<ActionResult<IEnumerable<Job>>> GetWorkerJobs(string id)
        {
            var jobs = await _workerService.GetWorkerJobsAsync(id);
            if (jobs == null)
                return NotFound();

            return Ok(jobs);
        }


        [HttpPut("{jobId}/jobstatus")]
        public async Task<IActionResult> UpdateJobStatus(int jobId, [FromBody] JobStatus status)
        {
            var res = await _jobProgressService.UpdateJobStatus(jobId, status);
            return res ? Ok() : NotFound();
        }
        

        [HttpPut("{jobId}/progress")]
        public async Task<IActionResult> UpdateJobProgress(int jobId, [FromBody] int progress)
        {
            var jobStatusDto = await _jobProgressService.UpdateJobProgress(jobId, progress);
            if (jobStatusDto == null)
                return NotFound();

            await _jobNotifier.NotifyProgressAsync(jobStatusDto); // send to JobsServer.Api

            return Ok();
        }

    }
}
