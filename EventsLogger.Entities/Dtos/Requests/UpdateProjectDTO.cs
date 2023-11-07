namespace EventsLogger.Entities.Dtos.Requests;

public class UpdateProjectDTO
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required CreateAddressDTO Address { get; set; }

}
