using Microsoft.AspNetCore.Http;

namespace EventsLogger.Entities.Dtos.Requests;

public class CreateEntryDTO
{
    public string Description { get; init; } = string.Empty;
    public List<IFormFile>? Files { get; init; }
}