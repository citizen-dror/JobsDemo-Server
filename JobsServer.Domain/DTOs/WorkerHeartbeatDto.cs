using JobsServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Domain.DTOs
{
    public class WorkerHeartbeatDto
    {
        public string WorkerId { get; set; }
        public WorkerStatus Status { get; set; }
        public int ActiveJobCount { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
