namespace EventsLogger.Entities.Dtos.Response;

public class ProjectDTO
{
    public ProjectDTO()
    {
        Users = new HashSet<UserProjectRoleDTO>();
    }
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public required AddressDTO Address { get; set; }
    public DateTime CreatedDate { get; set; }
    public ICollection<UserProjectRoleDTO> Users { get; set; }

}
