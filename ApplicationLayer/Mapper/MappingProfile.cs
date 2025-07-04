using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.DTO.CinemaRoomManagement;
using ApplicationLayer.DTO.EmployeeManagement;
using ApplicationLayer.DTO.JWT;
using ApplicationLayer.DTO.MovieManagement;
using ApplicationLayer.DTO.PromotionManagement;
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
            //Movie
            CreateMap<MovieCreateDto, Movie>();

            //Movie Image
            CreateMap<MovieImageDto, MovieImage>();

            //ShowTime
            CreateMap<ShowTimeDto, ShowTime>();

            //View Movie
            CreateMap<Movie, MovieResponseDto>()
                .ForMember(dest => dest.Images,
                    opt => opt.MapFrom(src => src.MovieImages.Select(img => new MovieImageDto
                    {
                        ImageUrl = img.ImageUrl,
                        Description = img.Description,
                        DisplayOrder = img.DisplayOrder,
                        IsPrimary = img.IsPrimary
                    }).ToList()))
                .ForMember(dest => dest.Genres,
                    opt => opt.MapFrom(src => src.MovieGenres.Select(mg => new GenreDto
                    {
                        Id = mg.Genre.Id,
                        Name = mg.Genre.GenreName,
                        Description = mg.Genre.Description
                    }).ToList()))
                .AfterMap((src, dest) =>
                {
                    var primary = src.MovieImages.FirstOrDefault(i => i.IsPrimary);
                    dest.PrimaryImageUrl = primary != null ? primary.ImageUrl : null;
                });


            //Update Movie
            CreateMap<MovieUpdateDto, Movie>()
                .ForMember(dest => dest.MovieGenres, opt => opt.Ignore())
                .ForMember(dest => dest.ShowTimes, opt => opt.Ignore())
                .ForMember(dest => dest.MovieImages, opt => opt.Ignore());

            // Add these to your existing MappingProfile class
            CreateMap<PromotionCreateDto, Promotion>();
            CreateMap<PromotionUpdateDto, Promotion>();
            CreateMap<Promotion, PromotionResponseDto>();

            //Search Movie
            CreateMap<Movie, MovieListDto>();

            //Genre
            CreateMap<Genre, GenreListDto>();
            CreateMap<GenreCreateDto, Genre>();

            //Cinema Room
            CreateMap<CinemaRoom, CinemaRoomListDto>();

            //Seat
            CreateMap<Seat, SeatViewDto>();

            //Booking
            CreateMap<Movie, MovieDropdownDto>();

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
        }
    }
}
