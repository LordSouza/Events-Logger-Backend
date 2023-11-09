namespace EventsLogger.BlobService.Repositories.Interfaces;

public interface IQueuesManagement
{
    public Task<bool> SendMessage<T>(T serviceMessage, string queue, string connectionString);
}