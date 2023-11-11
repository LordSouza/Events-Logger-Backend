namespace EventsLogger.Entities.Dtos.Response;

public class UserPublicDTO
{
    public UserPublicDTO()
    {
        UserEntries = new HashSet<EntryPublicDTO>();
    }
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhotoPath { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public ICollection<EntryPublicDTO> UserEntries { get; set; }
}