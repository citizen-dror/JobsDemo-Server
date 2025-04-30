using JobQueueSystem.Core.DTOs;
using JobQueueSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Domain.Interfaces.Services
{
    public interface IJobProgressService
    {
        Task<JobStatusDto> UpdateJobProgress(int id, int progress);
        Task<bool> UpdateJobStatus(int id, JobStatus status);
    }
}
