namespace EventsLogger.Entities.Dtos.Response;

public class EntryDTO
{
    public EntryDTO()
    {
        FilesUrl = new();
    }
    public Guid Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Description { get; init; }
    public List<string> FilesUrl { get; init; }
    public UserDTO? UserDTO { get; set; }
    public ProjectDTO? ProjectDTO { get; set; }
}
