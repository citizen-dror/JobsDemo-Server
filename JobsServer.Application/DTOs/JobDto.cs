using JobsServer.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Application.DTOs
{
    public class JobDto
    {
        public int Id { get; set; }
        public string JobName { get; set; } = string.Empty;
        public JobPriority Priority { get; set; }
        public JobStatus Status { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Progress { get; set; } = 0;
    }
}
