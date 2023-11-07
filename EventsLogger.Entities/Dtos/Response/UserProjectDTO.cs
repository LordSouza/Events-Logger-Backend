namespace EventsLogger.Entities.Dtos.Response;

public class UserProjectDTO
{
    public required Guid ProjectId { get; init; }
    public required Guid UserId { get; init; }
    public required string Role { get; set; }
}
