namespace EventsLogger.Entities.Dtos.Response;

public class EntryPublicDTO
{
    public EntryPublicDTO()
    {
        FilesUrl = new();
    }
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Description { get; init; }
    public List<string> FilesUrl { get; init; }
}