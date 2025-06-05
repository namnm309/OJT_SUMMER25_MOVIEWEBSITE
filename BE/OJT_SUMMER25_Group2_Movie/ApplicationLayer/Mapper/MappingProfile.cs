using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLayer.DTO.Auth;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;

namespace ApplicationLayer.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            //Auth - User
            CreateMap<Register, Users>()
            // Chuyển string Phone sang int (nên validate trước khi map)
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => int.Parse(src.Phone)))

            // Set Role mặc định là Member
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => UserRole.Member))

            // Set IsActive mặc định true
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))

            // Bỏ qua các trường không có trong DTO hoặc không map
            .ForMember(dest => dest.Score, opt => opt.Ignore())
            .ForMember(dest => dest.Avatar, opt => opt.Ignore())

            // Nếu có trường Password trong entity, bạn sẽ hash riêng trong service nên không map thẳng
            .ForMember(dest => dest.Password, opt => opt.Ignore());
        }
    }
}
