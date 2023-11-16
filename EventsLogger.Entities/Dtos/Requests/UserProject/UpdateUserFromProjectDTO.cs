using Microsoft.AspNetCore.Http;
using Microsoft.Win32.SafeHandles;

namespace EventsLogger.Entities.Dtos.Requests.UserProject;

public class UpdateUserFromProjectDTO
{
    public Guid ProjectId { get; set; }
    public required string UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public IFormFile? File { get; init; }
}