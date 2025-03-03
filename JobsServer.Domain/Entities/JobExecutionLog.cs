using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Domain.Entities
{
    public class JobExecutionLog
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? WorkerId { get; set; }
    }
}
