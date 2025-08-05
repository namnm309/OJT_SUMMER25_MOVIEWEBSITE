using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.DTO.CinemaRoomManagement;
using ApplicationLayer.DTO.EmployeeManagement;
using ApplicationLayer.DTO.JWT;
using ApplicationLayer.DTO.MovieManagement;
using ApplicationLayer.DTO.PromotionManagement;
using ApplicationLayer.DTO.ShowtimeManagement;
using ApplicationLayer.DTO.TicketSellingManagement;
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
            CreateMap<MovieCreateDto, Movie>()
                .ForMember(dest => dest.MovieActors, opt => opt.Ignore())
                .ForMember(dest => dest.MovieDirectors, opt => opt.Ignore());
            
            //Movie Update - include new properties
            CreateMap<MovieUpdateDto, Movie>()
                .ForMember(dest => dest.MovieGenres, opt => opt.Ignore())
                .ForMember(dest => dest.ShowTimes, opt => opt.Ignore())
                .ForMember(dest => dest.MovieImages, opt => opt.Ignore())
                .ForMember(dest => dest.MovieActors, opt => opt.Ignore())
                .ForMember(dest => dest.MovieDirectors, opt => opt.Ignore());

            //Movie Image
            CreateMap<MovieImageDto, MovieImage>();

            //ShowTime
            CreateMap<ShowTimeDto, ShowTime>();

            //View Movie
            // Mapping for Actor & Director
            CreateMap<Actor, ActorDto>();
            CreateMap<Director, DirectorDto>();
            CreateMap<ActorCreateDto, Actor>();
            CreateMap<DirectorCreateDto, Director>();

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
                .ForMember(dest => dest.ActorList,
                    opt => opt.MapFrom(src => src.MovieActors.Select(ma => new ActorDto
                    {
                        Id = ma.Actor.Id,
                        Name = ma.Actor.Name
                    }).ToList()))
                .ForMember(dest => dest.DirectorList,
                    opt => opt.MapFrom(src => src.MovieDirectors.Select(md => new DirectorDto
                    {
                        Id = md.Director.Id,
                        Name = md.Director.Name
                    }).ToList()))
                .ForMember(dest => dest.Director,
                    opt => opt.MapFrom(src => string.Join(", ", src.MovieDirectors.Select(md => md.Director.Name))))
                .ForMember(dest => dest.Actors,
                    opt => opt.MapFrom(src => string.Join(", ", src.MovieActors.Select(ma => ma.Actor.Name))))
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
            CreateMap<Seat, SeatDto>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.PriceSeat))
                .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => src.SeatType.ToString()));
            CreateMap<SeatUpdateDto, Seat>();

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
                .ForMember(dest => dest.BookedSeats, opt => opt.MapFrom(src => src.Bookings.Sum(b => b.BookingDetails.Count)))
                // Convert stored UTC date back to local date for displaying in UI
                .ForMember(dest => dest.ShowDate, opt => opt.MapFrom(src =>
                    src.ShowDate.HasValue
                        ? DateTime.SpecifyKind(src.ShowDate.Value.ToLocalTime().Date, DateTimeKind.Unspecified)
                        : DateTime.MinValue));

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


            // Ticket
            CreateMap<Ticket, TicketDto>();
            
            // Admin Booking Detail
            CreateMap<Booking, AdminBookingDetailDto>()
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.ShowTime.Movie.Title))
                .ForMember(dest => dest.CinemaRoom, opt => opt.MapFrom(src => src.ShowTime.Room.RoomName))
                .ForMember(dest => dest.ShowDate, opt => opt.MapFrom(src => src.ShowTime.ShowDate))
                .ForMember(dest => dest.ShowTime, opt => opt.MapFrom(src => src.ShowTime.ShowDate.HasValue ? src.ShowTime.ShowDate.Value.ToString("HH:mm") : ""))
                .ForMember(dest => dest.SeatCodes, opt => opt.MapFrom(src => src.BookingDetails.Select(bd => bd.Seat.SeatCode).ToList()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.User.Phone))
                .ForMember(dest => dest.UserIdentityCard, opt => opt.MapFrom(src => src.User.IdentityCard));
        }
    }
}
