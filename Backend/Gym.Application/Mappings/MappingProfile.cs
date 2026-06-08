using AutoMapper;
using Gym.Application.DTOs.Gyms;
using Gym.Application.DTOs.Members;
using Gym.Application.DTOs.Privileges;
using Gym.Application.DTOs.Roles;
using Gym.Application.DTOs.Trainers;
using Gym.Domain.Entities;

namespace Gym.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Role, RoleDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CreatedDate, o => o.MapFrom(s => s.CreatedDate));

        CreateMap<Privilege, PrivilegeDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CreatedDate, o => o.MapFrom(s => s.CreatedDate));

        CreateMap<Gym.Domain.Entities.Gym, GymDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id));

        CreateMap<Trainer, TrainerDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id));

        CreateMap<Member, MemberDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id));
    }
}
