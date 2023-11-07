namespace EventsLogger.Entities.Dtos.Requests;

public class UpdateEntryDTO
{
    public Guid Id { get; set; }
    public string? Description { get; init; }
    public string[]? Files { get; init; }
}