using Microsoft.AspNetCore.Http;

namespace EventsLogger.Entities.Dtos.Requests.Entry;

public class CreateEntryDTO
{
    public CreateEntryDTO()
    {
        Files = new List<IFormFile>();
    }
    public Guid ProjectId { get; set; }
    public string Description { get; init; } = string.Empty;

    public List<IFormFile> Files { get; init; }

}