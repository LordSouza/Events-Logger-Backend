namespace EventsLogger.Entities.Dtos.Requests;

public class UpdateUserDTO
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhotoPath { get; set; }
}
