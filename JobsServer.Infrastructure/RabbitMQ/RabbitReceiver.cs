using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace JobsServer.Infrastructure.RabbitMQ
{
    public class RabbitReceiver
    {
        private readonly RabbitConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitReceiver> _logger;
        private readonly string _exchangeName;
        private readonly string _routingKey;
        private readonly bool _durable;
        private readonly bool _exclusive;
        private readonly bool _autoDelete;

        public RabbitReceiver(
            RabbitConnectionFactory connectionFactory,
            ILogger<RabbitReceiver> logger,
            string exchangeName = "JobWorker1",
            string routingKey = "job-rkey1",
            bool durable = false,
            bool exclusive = false,
            bool autoDelete = false)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            _exchangeName = exchangeName;
            _routingKey = routingKey;
            _durable = durable;
            _exclusive = exclusive;
            _autoDelete = autoDelete;
        }

        public async Task StartReceivingAsync(string queueName, Func<string, Task> onMessageReceived, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                _logger.LogError("Queue name is required.");
                throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));
            }

            if (onMessageReceived == null)
            {
                _logger.LogError("Message handler is required.");
                throw new ArgumentNullException(nameof(onMessageReceived));
            }

            try
            {
                _logger.LogInformation("Establishing connection to RabbitMQ server...");

                var connection = await _connectionFactory.CreateConnectionAsync();
                var channel = await _connectionFactory.CreateChannelAsync(connection);

                await channel.ExchangeDeclareAsync(_exchangeName, "direct");
                await channel.QueueDeclareAsync(queue: queueName, durable: _durable, exclusive: _exclusive, autoDelete: _autoDelete, arguments: null);
                await channel.QueueBindAsync(queue: queueName, exchange: _exchangeName, routingKey: _routingKey, arguments: null);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation($"Received message: {message}");
                    return Task.CompletedTask;
                };
                _logger.LogInformation("Starting consumer...");

                await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

                // Keep the task alive until cancelled
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Receiving canceled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while receiving messages: {ex.Message}");
                throw new InvalidOperationException($"Error receiving messages from RabbitMQ: {ex.Message}", ex);
            }
        }
    }

}
