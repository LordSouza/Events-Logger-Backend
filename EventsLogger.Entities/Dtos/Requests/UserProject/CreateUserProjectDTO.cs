namespace EventsLogger.Entities.Dtos.Requests.UserProject;

public class CreateUserProjectDTO
{
    public required Guid ProjectId { get; init; }
    public required string UserId { get; init; }
    public required string Role { get; set; }
}
