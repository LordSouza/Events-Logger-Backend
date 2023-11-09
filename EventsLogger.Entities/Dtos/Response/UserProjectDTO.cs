namespace EventsLogger.Entities.Dtos.Response;

public class UserProjectDTO
{
    public required Guid ProjectId { get; init; }
    public ProjectDTO? Project { get; set; }
    public required Guid UserId { get; init; }
    public required string Role { get; set; }
}
