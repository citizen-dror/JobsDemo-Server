using JobsServer.Api.Models;
using JobsServer.Application.DTOs;
using JobsServer.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobsServer.Api.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs() => Ok(await _jobService.GetAllJobsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            return job == null ? NotFound() : Ok(job);
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto jobDto)
        {
            var job = await _jobService.CreateJobAsync(jobDto);
            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
        }

        [HttpPost("updateProgress")]
        public async Task<IActionResult> UpdateJobProgress([FromBody] JobProgressUpdateRequest request)
        {
            var res = await _jobService.UpdateJobProgress(request.JobId, request.Progress);
            return res ? Ok() : NotFound();
        }

        [HttpPut("{id}/stop")]
        public async Task<IActionResult> StopJob(int id) => await _jobService.StopJobAsync(id) ? Ok() : NotFound();

        [HttpPut("{id}/restart")]
        public async Task<IActionResult> RestartJob(int id) => await _jobService.RestartJobAsync(id) ? Ok() : NotFound();

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id) => await _jobService.DeleteCompletedOrFailedJobAsync(id) ? Ok() : NotFound();
    }
}
