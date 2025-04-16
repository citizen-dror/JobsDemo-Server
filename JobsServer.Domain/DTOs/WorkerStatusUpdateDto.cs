using JobsServer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Domain.DTOs
{
    public class WorkerStatusUpdateDto
    {
        public WorkerStatus Status { get; set; }
    }
}
