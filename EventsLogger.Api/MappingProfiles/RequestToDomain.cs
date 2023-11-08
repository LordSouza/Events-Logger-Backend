using AutoMapper;
using EventsLogger.Entities.DbSet;
using EventsLogger.Entities.Dtos.Requests;

namespace EventsLogger.Api.MappingProfiles;

public class RequestToDomain : Profile
{
    public RequestToDomain()
    {
        // create
        CreateMap<CreateUserDTO, User>()
        .ForMember(
            dest => dest.Id,
            opt => opt.MapFrom(src => Guid.NewGuid()))
        .ForMember(
            des => des.Status,
            opt => opt.MapFrom(src => 1))
        .ForMember(
            des => des.CreatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(
            des => des.UpdatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<CreateAddressDTO, Address>()
        .ForMember(
            dest => dest.Id,
            opt => opt.MapFrom(src => Guid.NewGuid()))
        .ForMember(
            des => des.Status,
            opt => opt.MapFrom(src => 1))
        .ForMember(
            des => des.CreatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(
            des => des.UpdatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<CreateProjectDTO, Project>()
        .ForMember(
            dest => dest.Id,
            opt => opt.MapFrom(src => Guid.NewGuid()))
        .ForMember(
            des => des.Status,
            opt => opt.MapFrom(src => 1))
        .ForMember(
            des => des.CreatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(
            des => des.UpdatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<CreateAddressDTO, Address>()
        .ForMember(
            dest => dest.Id,
            opt => opt.MapFrom(src => Guid.NewGuid()))
        .ForMember(
            des => des.Status,
            opt => opt.MapFrom(src => 1))
        .ForMember(
            des => des.CreatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(
            des => des.UpdatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<CreateUserProjectDTO, UserProject>()
        .ForMember(
            dest => dest.Id,
            opt => opt.MapFrom(src => Guid.NewGuid()))
        .ForMember(
            des => des.Status,
            opt => opt.MapFrom(src => 1))
        .ForMember(
            des => des.CreatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow))
        .ForMember(
            des => des.UpdatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow));


        // Update

        CreateMap<UpdateUserDTO, User>()
        .ForMember(
            dest => dest.Id,
            opt => opt.Ignore())
        .ForMember(
            des => des.Status,
            opt => opt.Ignore())
        .ForMember(
            des => des.CreatedDate,
            opt => opt.Ignore())
        .ForMember(
            dest => dest.Name,
            opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
        .ForMember(
            dest => dest.UserName,
            opt => opt.MapFrom(src => src.FirstName + "_" + src.LastName))
        .ForMember(
            des => des.UpdatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateAddressDTO, Address>()
        .ForMember(
            dest => dest.Id,
            opt => opt.Ignore())
        .ForMember(
            des => des.Status,
            opt => opt.Ignore())
        .ForMember(
            des => des.CreatedDate,
            opt => opt.Ignore())
        .ForMember(
            des => des.UpdatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateProjectDTO, Project>()
        .ForMember(
            dest => dest.Id,
            opt => opt.Ignore())
        .ForMember(
            des => des.Status,
            opt => opt.Ignore())
        .ForMember(
            des => des.CreatedDate,
            opt => opt.Ignore())
        .ForMember(
            des => des.UpdatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateAddressDTO, Address>()
        .ForMember(
            dest => dest.Id,
            opt => opt.Ignore())
        .ForMember(
            des => des.Status,
            opt => opt.Ignore())
        .ForMember(
            des => des.CreatedDate,
            opt => opt.Ignore())
        .ForMember(
            des => des.UpdatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<UpdateUserProjectDTO, UserProject>()
        .ForMember(
            dest => dest.Id,
            opt => opt.Ignore())
        .ForMember(
            des => des.Status,
            opt => opt.Ignore())
        .ForMember(
            des => des.CreatedDate,
            opt => opt.Ignore())
        .ForMember(
            des => des.UpdatedDate,
            opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}
