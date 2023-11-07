namespace EventsLogger.Entities.Dtos.Response;

public class ProjectNameDTO
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public DateTime CreatedDate { get; set; }

}
