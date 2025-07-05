using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLayer.DTO.BookingTicketManagement;
using AutoMapper;
using DomainLayer.Entities;

namespace ApplicationLayer.Mappings
{
    public class BookingProfile : Profile
    {
        public BookingProfile()
        {
            CreateMap<Users, MemberInfoDto>()
                .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.IdentityNumber, opt => opt.MapFrom(src => src.IdentityCard))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone));
        }
    }
}
