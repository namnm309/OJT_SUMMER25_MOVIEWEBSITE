using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLayer.DTO.CinemaRoomManagement;
using AutoMapper;
using DomainLayer.Entities;

namespace ApplicationLayer.Mappings
{
    public class SeatMappingProfile : Profile
    {
        public SeatMappingProfile()
        {
            CreateMap<Seat, SeatDto>()
                .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => src.SeatType.ToString()));
        }
    }
}
