using System.ComponentModel.DataAnnotations;

namespace EventsLogger.Entities.Dtos.Requests;

public class UserLoginRequestDTO
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}