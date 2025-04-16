﻿using JobsServer.Application.Services;
using JobsServer.Domain.DTOs;
using JobsServer.Domain.Entities;
using JobsServer.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobsServer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkerController : ControllerBase
    {
        private readonly IWorkerService _workerService;
        private readonly ILogger<WorkerController> _logger;

        public WorkerController(IWorkerService workerService, ILogger<WorkerController> logger)
        {
            _workerService = workerService;
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
            return null;
        }
        [HttpPost("{id}/jobs")]
        public async Task<ActionResult<bool>> AssignJobToWorker(string id, [FromBody] Job job)
        {
            return null;
        }
        [HttpGet("{id}/jobs")]
        public async Task<ActionResult<IEnumerable<Job>>> GetWorkerJobs(string id)
        {
            return null;
        }
       

    }
}
