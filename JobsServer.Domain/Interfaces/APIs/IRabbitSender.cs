using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Domain.Interfaces.APIs
{
    public interface IRabbitSender
    {
        Task SendMessageAsync(string queueName, string message, string? routingKeyOverride = null);
    }
}
