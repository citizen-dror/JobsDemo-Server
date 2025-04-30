using JobQueueSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobQueueSystem.Core.DTOs
{
    public class JobStatusDto
    {
        public int Id { get; set; }
        public int Progress { get; set; }
        public JobStatus Status { get; set; }
    }
}
