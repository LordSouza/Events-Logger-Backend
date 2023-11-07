namespace EventsLogger.Entities.Dtos.Requests;

public class UpdateUserProjectDTO
{
    public required Guid ProjectId { get; init; }
    public required Guid UserId { get; init; }
    public required string Role { get; set; }
}
