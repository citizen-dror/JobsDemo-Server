using JobsServer.Domain.Interfaces.APIs;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace JobsServer.Infrastructure.RabbitMQ
{
    public class RabbitSender: IRabbitSender
    {
        private readonly RabbitConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitSender> _logger;
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private readonly bool _durable;
        private readonly bool _exclusive;
        private readonly bool _autoDelete;

        // Constructor with Dependency Injection for Logger and configurable values
        public RabbitSender(RabbitConnectionFactory connectionFactory, ILogger<RabbitSender> logger,
            string exchangeName = "worker.jobs", string routingKey = "job-rkey1", bool durable = true, bool exclusive = false, bool autoDelete = false)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            _exchangeName = exchangeName;
            _routingKey = routingKey;
            _durable = durable;
            _exclusive = exclusive;
            _autoDelete = autoDelete;
        }

        public async Task SendMessageAsync(string queueName, string message, string? routingKeyOverride = null)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                _logger.LogError("Queue name is required.");
                throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogError("Message is required.");
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            }

            try
            {
                _logger.LogInformation($"Establishing connection to RabbitMQ server...");

                using (var connection = await _connectionFactory.CreateConnectionAsync())
                using (var channel = await _connectionFactory.CreateChannelAsync(connection))
                {
                    await channel.ExchangeDeclareAsync(_exchangeName, type: "direct");
                    await channel.QueueDeclareAsync(queue: queueName, durable: _durable, exclusive: _exclusive, autoDelete: _autoDelete, arguments: null);
                    await channel.QueueBindAsync(queueName, _exchangeName, routingKeyOverride ?? _routingKey, null);

                    var body = Encoding.UTF8.GetBytes(message);
                    _logger.LogInformation($"Publishing message to exchange {_exchangeName} with routing key {routingKeyOverride ?? _routingKey}...");

                    await channel.BasicPublishAsync(exchange: _exchangeName, routingKey: routingKeyOverride ?? _routingKey, body: body);

                    _logger.LogInformation($"Message successfully sent to queue {queueName}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while sending message to queue {queueName}: {ex.Message}");
                throw new InvalidOperationException($"Error sending message to RabbitMQ: {ex.Message}", ex);
            }
        }
    }
}
