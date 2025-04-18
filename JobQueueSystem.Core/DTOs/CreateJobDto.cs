using JobQueueSystem.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobQueueSystem.Core.DTOs
{
    public class CreateJobDto
    {
        [Required(ErrorMessage = "JobName is required.")]
        [StringLength(255, MinimumLength = 4, ErrorMessage = "JobName must be between 4 and 255 characters.")]
        public string JobName { get; set; } = string.Empty;

        [Required(ErrorMessage = "JobPriority is required.")]
        [EnumDataType(typeof(JobPriority), ErrorMessage = "Invalid JobPriority. Allowed values are 10 (Regular) and 20 (High).")]
        public JobPriority Priority { get; set; }
    }
}
