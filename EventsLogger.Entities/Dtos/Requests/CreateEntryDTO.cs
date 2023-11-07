namespace EventsLogger.Entities.Dtos.Requests;

public class CreateEntryDTO
{
    public string Description { get; init; } = string.Empty;
    public string[]? Files { get; init; }
}