namespace EventsLogger.Entities.Dtos.Requests.UserProject;

public class DeleteUserFromProjectDTO
{
    public string UserId { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
}