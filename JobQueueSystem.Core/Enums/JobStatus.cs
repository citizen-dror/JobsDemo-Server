namespace JobQueueSystem.Core.Enums
{
    public enum JobStatus
    {
        Pending = 10,
        Scheduled = 15,
        InProgress = 20,
        Completed = 30,
        Failed = 40,
        Retrying = 50,
        Cancelled = 60
    }
}