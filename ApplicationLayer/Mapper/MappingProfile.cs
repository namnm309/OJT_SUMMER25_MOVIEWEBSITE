using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.DTO.CinemaRoomManagement;
using ApplicationLayer.DTO.EmployeeManagement;
using ApplicationLayer.DTO.JWT;
using ApplicationLayer.DTO.MovieManagement;
using ApplicationLayer.DTO.PromotionManagement;
using ApplicationLayer.DTO.ShowtimeManagement;
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
            //Movie
            CreateMap<MovieCreateDto, Movie>();
            
            //Movie Update - include new properties
            CreateMap<MovieUpdateDto, Movie>()
                .ForMember(dest => dest.MovieGenres, opt => opt.Ignore())
                .ForMember(dest => dest.ShowTimes, opt => opt.Ignore())
                .ForMember(dest => dest.MovieImages, opt => opt.Ignore());

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
            CreateMap<CinemaRoomCreateDto, CinemaRoom>();

            //Seat
            CreateMap<Seat, SeatViewDto>();
            CreateMap<Seat, SeatDetailDto>();

            //Showtime
            CreateMap<ShowtimeCreateDto, ShowTime>()
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.CinemaRoomId));
            
            CreateMap<ShowtimeUpdateDto, ShowTime>()
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.CinemaRoomId));
            
            CreateMap<ShowTime, ShowtimeListDto>()
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Movie.Title))
                .ForMember(dest => dest.MoviePoster, opt => opt.MapFrom(src => 
                    src.Movie.MovieImages.FirstOrDefault(img => img.IsPrimary) != null 
                        ? src.Movie.MovieImages.FirstOrDefault(img => img.IsPrimary)!.ImageUrl 
                        : src.Movie.MovieImages.FirstOrDefault() != null 
                            ? src.Movie.MovieImages.FirstOrDefault()!.ImageUrl 
                            : string.Empty))
                .ForMember(dest => dest.MovieDuration, opt => opt.MapFrom(src => src.Movie.RunningTime))
                .ForMember(dest => dest.CinemaRoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.CinemaRoomName, opt => opt.MapFrom(src => src.Room.RoomName))
                .ForMember(dest => dest.TotalSeats, opt => opt.MapFrom(src => src.Room.TotalSeats))
                .ForMember(dest => dest.BookedSeats, opt => opt.MapFrom(src => src.Bookings.Count));

            //Booking
            CreateMap<Movie, MovieDropdownDto>()
                .ForMember(dest => dest.PrimaryImageUrl, 
                    opt => opt.MapFrom(src => src.MovieImages.FirstOrDefault(img => img.IsPrimary) != null 
                        ? src.MovieImages.FirstOrDefault(img => img.IsPrimary)!.ImageUrl 
                        : src.MovieImages.FirstOrDefault() != null 
                            ? src.MovieImages.FirstOrDefault()!.ImageUrl 
                            : null))
                .ForMember(dest => dest.Genre,
                    opt => opt.MapFrom(src => string.Join(", ", src.MovieGenres.Select(mg => mg.Genre.GenreName))))
                .ForMember(dest => dest.Duration,
                    opt => opt.MapFrom(src => src.RunningTime));

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
