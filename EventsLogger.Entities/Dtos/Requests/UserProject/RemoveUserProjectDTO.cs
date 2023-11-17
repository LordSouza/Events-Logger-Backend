namespace EventsLogger.Entities.Dtos.Requests.UserProject;

public class RemoveUserProjectDTO
{
    public required Guid ProjectId { get; init; }
    public required string UserId { get; init; }
}
