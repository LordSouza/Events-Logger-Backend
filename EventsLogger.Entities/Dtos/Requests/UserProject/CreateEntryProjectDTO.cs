using Microsoft.AspNetCore.Http;

namespace EventsLogger.Entities.Dtos.Requests.UserProject;

public class CreateEntryProjectDTO
{
    public CreateEntryProjectDTO()
    {
        Files = new List<IFormFile>();
    }
    public string UserId { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string Description { get; init; } = string.Empty;

    public List<IFormFile> Files { get; init; }

}