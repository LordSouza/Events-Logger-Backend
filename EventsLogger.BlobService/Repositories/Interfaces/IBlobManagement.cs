namespace EventsLogger.BlobService.Repositories.Interfaces;

public interface IBlobManagement
{
    /// <summary>
    /// upload file to the blob storage
    /// </summary>
    /// <param name="containerName"></param>
    /// <param name="fileName"></param>
    /// <param name="file"></param>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    Task<string> UploadFile(string containerName, string fileName, byte[] file, string connectionString);
}