using JobsServer.Application.DTOs;
using JobsServer.Application.Services;
using JobsServer.Domain.Enums;
using Moq;

namespace JobsServer.Tests
{
    public class JobServiceTests
    {
        private readonly Mock<IJobService> _jobServiceMock;

        public JobServiceTests()
        {
            _jobServiceMock = new Mock<IJobService>();
        }

        [Fact]
        public async Task GetAllJobsAsync_ReturnsListOfJobs()
        {
            // Arrange
            var expectedJobs = new List<JobDto>
            {
                new JobDto { Id = 1, JobName = "Job A", Progress = 50, Priority = JobPriority.Regular , Status =JobStatus.InProgress},
                new JobDto { Id = 2, JobName = "Job B", Progress = 100, Priority = JobPriority.Regular, Status =JobStatus.InProgress }
            };

            _jobServiceMock.Setup(s => s.GetAllJobsAsync())
                           .ReturnsAsync(expectedJobs);

            // Act
            var result = await _jobServiceMock.Object.GetAllJobsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedJobs.Count, result.Count());
        }

        [Fact]
        public async Task GetJobByIdAsync_ReturnsJob_WhenJobExists()
        {
            // Arrange
            var job = new JobDto { Id = 1, JobName = "Job A", Progress = 50, Priority = JobPriority.Regular, Status = JobStatus.InProgress };
            _jobServiceMock.Setup(s => s.GetJobByIdAsync(1))
                           .ReturnsAsync(job);
            // Act
            var result = await _jobServiceMock.Object.GetJobByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(job.Id, result.Id);
        }

        [Fact]
        public async Task GetJobByIdAsync_ReturnsNull_WhenJobDoesNotExist()
        {
            // Arrange
            _jobServiceMock.Setup(s => s.GetJobByIdAsync(It.IsAny<int>()))
                           .ReturnsAsync((JobDto?)null);

            // Act
            var result = await _jobServiceMock.Object.GetJobByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateJobAsync_ReturnsCreatedJob()
        {
            // Arrange
            var createJobDto = new CreateJobDto { JobName = "New Job", Priority = JobPriority.Regular };
            var createdJob = new JobDto { Id = 1, JobName = "New Job", Priority = JobPriority.Regular,  Progress = 0 };

            _jobServiceMock.Setup(s => s.CreateJobAsync(createJobDto))
                           .ReturnsAsync(createdJob);

            // Act
            var result = await _jobServiceMock.Object.CreateJobAsync(createJobDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdJob.Id, result.Id);
            Assert.Equal(createJobDto.JobName, result.JobName);
            Assert.Equal(createJobDto.Priority, result.Priority);
        }

        [Fact]
        public async Task StopJobAsync_ReturnsTrue_WhenJobStoppedSuccessfully()
        {
            // Arrange
            _jobServiceMock.Setup(s => s.StopJobAsync(1))
                           .ReturnsAsync(true);

            // Act
            var result = await _jobServiceMock.Object.StopJobAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task StopJobAsync_ReturnsFalse_WhenJobNotStopped()
        {
            // Arrange
            _jobServiceMock.Setup(s => s.StopJobAsync(It.IsAny<int>()))
                           .ReturnsAsync(false);

            // Act
            var result = await _jobServiceMock.Object.StopJobAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RestartJobAsync_ReturnsTrue_WhenJobRestartedSuccessfully()
        {
            // Arrange
            _jobServiceMock.Setup(s => s.RestartJobAsync(1))
                           .ReturnsAsync(true);

            // Act
            var result = await _jobServiceMock.Object.RestartJobAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RestartJobAsync_ReturnsFalse_WhenJobNotRestarted()
        {
            // Arrange
            _jobServiceMock.Setup(s => s.RestartJobAsync(It.IsAny<int>()))
                           .ReturnsAsync(false);

            // Act
            var result = await _jobServiceMock.Object.RestartJobAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteCompletedOrFailedJobAsync_ReturnsTrue_WhenJobDeleted()
        {
            // Arrange
            _jobServiceMock.Setup(s => s.DeleteCompletedOrFailedJobAsync(1))
                           .ReturnsAsync(true);

            // Act
            var result = await _jobServiceMock.Object.DeleteCompletedOrFailedJobAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteCompletedOrFailedJobAsync_ReturnsFalse_WhenJobNotDeleted()
        {
            // Arrange
            _jobServiceMock.Setup(s => s.DeleteCompletedOrFailedJobAsync(It.IsAny<int>()))
                           .ReturnsAsync(false);

            // Act
            var result = await _jobServiceMock.Object.DeleteCompletedOrFailedJobAsync(99);

            // Assert
            Assert.False(result);
        }
    }
}
