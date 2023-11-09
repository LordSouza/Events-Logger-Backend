using EventsLogger.BlobService.Repositories.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace EventsLogger.BlobService.Repositories;

public class BlobManagement : IBlobManagement
{
    public async Task<string> UploadFile(
        string containerName,
        string fileName,
        byte[] file,
        string connectionString)
    {
        try
        {

            // create a container reference
            var container = new BlobContainerClient(connectionString, containerName);
            await container.CreateIfNotExistsAsync();

            await container.SetAccessPolicyAsync(PublicAccessType.Blob);

            var blob = container.GetBlobClient(fileName);

            Stream stream = new MemoryStream(file);

            await blob.UploadAsync(stream);

            return blob.Uri.AbsoluteUri;
        }
        catch (System.Exception)
        {

            throw;
        }
    }
}