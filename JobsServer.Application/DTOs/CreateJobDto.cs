using JobsServer.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsServer.Application.DTOs
{
    public class CreateJobDto
    {
        [Required]
        public string JobName { get; set; } = string.Empty;

        [Required]
        public JobPriority Priority { get; set; }
    }
}
