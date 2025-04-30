using JobQueueSystem.Core.DTOs;
using JobsServer.Api.SignalR;
using JobsServer.Domain.Interfaces.APIs;
using JobsServer.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace JobsServer.Api.Controllers
{
    [ApiController]
    [Route("internal/notifications")]
    public class JobNotificationsController : ControllerBase
    {
        private readonly IJobUpdateNotifier _jobUpdateNotifier;

        public JobNotificationsController(IJobUpdateNotifier jobUpdateNotifier)
        {
            _jobUpdateNotifier = jobUpdateNotifier;
        }

        [HttpPost("updateProgress")]
        public async Task<IActionResult> NotifyJobUpdated([FromBody] JobStatusDto jobStatus)
        {
            await _jobUpdateNotifier.NotifyJobUpdate(jobStatus);
            return Ok();
        }
    }
}
