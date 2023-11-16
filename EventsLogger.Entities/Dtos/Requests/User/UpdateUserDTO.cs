using Microsoft.AspNetCore.Http;

namespace EventsLogger.Entities.Dtos.Requests.User;

public class UpdateUserDTO
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? NewPassword { get; set; }
    public IFormFile? File { get; init; }

}
