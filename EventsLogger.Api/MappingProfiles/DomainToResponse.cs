using AutoMapper;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Response;

namespace EventsLogger.Api.MappingProfiles;

public class DomainToResponse : Profile
{
    public DomainToResponse()
    {
        CreateMap<User, UserDTO>();
        CreateMap<User, UserProjectRoleDTO>();
        CreateMap<Address, AddressDTO>();
        CreateMap<Project, ProjectDTO>();
        CreateMap<Project, ProjectNameDTO>();
        CreateMap<Entry, EntryDTO>();
        CreateMap<UserProject, UserProjectDTO>();
    }
}
