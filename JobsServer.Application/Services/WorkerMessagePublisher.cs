using JobsServer.Domain.DTOs;
using JobsServer.Domain.Interfaces.APIs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace JobsServer.Application.Services
{
    public class WorkerMessagePublisher : IWorkerMessagePublisher
    {
        private readonly IRabbitSender _rabbitSender;
        private readonly ILogger<WorkerMessagePublisher> _logger;

        public WorkerMessagePublisher(IRabbitSender rabbitSender, ILogger<WorkerMessagePublisher> logger)
        {
            _rabbitSender = rabbitSender;
            _logger = logger;
        }

        public async Task PublishControlMessageAsync(string workerId, JobControlMessage message)
        {
            try
            {
                var json = JsonConvert.SerializeObject(message);
                var queueName = $"worker.{workerId}";
                await _rabbitSender.SendMessageAsync(queueName, json, routingKeyOverride: queueName);
                _logger.LogInformation($"Published {message.Type} message for job {message.JobId} to worker {workerId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish {message.Type} for job {message.JobId} to worker {workerId}");
                throw;
            }
        }
    }

}
