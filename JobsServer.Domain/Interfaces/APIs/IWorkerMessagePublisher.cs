using JobsServer.Domain.DTOs;

namespace JobsServer.Domain.Interfaces.APIs
{
    public interface IWorkerMessagePublisher
    {
        Task PublishControlMessageAsync(string workerId, JobControlMessage message);
    }
}
