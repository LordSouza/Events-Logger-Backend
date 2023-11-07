namespace EventsLogger.Entities.Dtos.Requests;

public class CreateProjectDTO
{
    public required string Name { get; set; }
    public required CreateAddressDTO Address { get; set; }
}
