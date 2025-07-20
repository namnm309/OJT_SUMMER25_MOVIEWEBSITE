using ApplicationLayer.DTO.EmployeeManagement;
using ApplicationLayer.DTO.JWT;
using ApplicationLayer.DTO.UserManagement;
using AutoMapper;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Employee
            CreateMap<EmployeeCreateDto, Employee>();
            CreateMap<Employee, EmployeeListDto>();
            CreateMap<EmployeeUpdateDto, Employee>().ReverseMap();

            //Auth - User
            CreateMap<RegisterReq, Users>();
            CreateMap<Users, UserDto>()
                .ForMember(dest => dest.IdentityCard, opt => opt.MapFrom(src => src.IdentityCard))
                .ForMember(dest => dest.Score, opt => opt.MapFrom(src => src.Score))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<Users, LoginResp>();
            CreateMap<EditUserReq, Users>().ReverseMap();

            CreateMap<Users, CustomerSearchDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone ?? string.Empty))
                .ForMember(dest => dest.Points, opt => opt.MapFrom(src => (int)Math.Round(src.Score)));

            // User Management
            CreateMap<RegisterRequestDto, Users>();
            CreateMap<Users, UserResponseDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
            CreateMap<UserCreateDto, Users>();
            CreateMap<UserUpdateDto, Users>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
