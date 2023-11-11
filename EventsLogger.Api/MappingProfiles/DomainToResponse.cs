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
        CreateMap<User, UserPublicDTO>();
        CreateMap<Address, AddressDTO>();
        CreateMap<Project, ProjectDTO>();
        CreateMap<Project, ProjectNameDTO>();
        CreateMap<Entry, EntryPublicDTO>();
        CreateMap<Entry, EntryDTO>()
        .ForMember(
            dest => dest.UserDTO,
            opt => opt.MapFrom(src => src.User))
        .ForMember(
        dest => dest.ProjectDTO,
        opt => opt.MapFrom(src => src.Project));
        CreateMap<UserProject, UserProjectDTO>();
    }
}
