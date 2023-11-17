using System.ComponentModel.DataAnnotations;

namespace EventsLogger.Entities.Dtos.Requests.Auth;

public class UserRegistrationRequestDTO
{
    [Required]
    public required string UserName { get; set; }

    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}