using Microsoft.AspNetCore.Http;

namespace EventsLogger.Entities.Dtos.Requests;

public class UpdateUserDTO
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhotoPath { get; set; }


}
