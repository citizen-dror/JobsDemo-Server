using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobQueueSystem.Core.Configs;
using RabbitMQ.Client;

namespace JobsServer.Infrastructure.RabbitMQ
{
    public class RabbitConnectionFactory
    {
        private readonly ConnectionFactory _factory;

        /// <summary>
        /// Creates a new RabbitMQ connection factory with multiple configuration options
        /// </summary>
        public RabbitConnectionFactory(RabbitMQConfig config)
        {
            _factory = new ConnectionFactory
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password,
                VirtualHost = config.VirtualHost,
                Port = config.Port,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };
        }

        /// <summary>
        /// Creates a RabbitMQ connection factory using a full AMQP URI
        /// </summary>
        /// <param name="amqpUri">Full AMQP connection URI</param>
        public RabbitConnectionFactory(string amqpUri)
        {
            _factory = new ConnectionFactory
            {
                Uri = new Uri(amqpUri),
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };
        }

        /// <summary>
        /// Asynchronously creates and returns a RabbitMQ connection
        /// </summary>
        public async Task<IConnection> CreateConnectionAsync()
        {
            try
            {
                return await Task.Run(() => _factory.CreateConnectionAsync());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create RabbitMQ connection", ex);
            }
        }

        /// <summary>
        /// Asynchronously creates a RabbitMQ channel using the provided connection
        /// </summary>
        public async Task<IChannel> CreateChannelAsync(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection), "Connection cannot be null");
            }

            try
            {
                return await connection.CreateChannelAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create RabbitMQ channel", ex);
            }
        }
    }
}
