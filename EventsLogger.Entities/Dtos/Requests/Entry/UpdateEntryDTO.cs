using Microsoft.AspNetCore.Http;

namespace EventsLogger.Entities.Dtos.Requests.Entry;

public class UpdateEntryDTO
{
    public UpdateEntryDTO()
    {
        Files = new List<IFormFile>();
    }
    public string? Description { get; init; }
    public List<IFormFile> Files { get; init; }
}