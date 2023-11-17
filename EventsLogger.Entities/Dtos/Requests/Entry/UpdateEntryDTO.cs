namespace EventsLogger.Entities.Dtos.Requests.Entry;

public class UpdateEntryDTO
{
    public Guid Id { get; set; }
    public string? Description { get; init; }
    public string[]? Files { get; init; }
}