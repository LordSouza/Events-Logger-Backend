
using System.Text.Json;
using Azure.Storage.Queues;
using EventsLogger.BlobService.Repositories.Interfaces;

namespace EventsLogger.BlobService.Repositories;

public class QueuesManagement : IQueuesManagement
{
    public async Task<bool> SendMessage<T>(T serviceMessage, string queue, string connectionString)
    {
        try
        {
            var queueClient = new QueueClient(connectionString, queue);

            var msgBody = JsonSerializer.Serialize(serviceMessage);

            await queueClient.SendMessageAsync(msgBody);

            return true;
        }
        catch (Exception)
        {
            return false;
        }

    }
}