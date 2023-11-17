namespace EventsLogger.Entities.Dtos.Requests.UserProject;

public class CreateNewUserProjectDTO
{
    public required Guid ProjectId { get; init; }
    public required string Role { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhotoPath { get; set; }
    public string? UserName { get; set; }

}
