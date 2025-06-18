using ApplicationLayer.DTO.CinemaRoomManagement;
using ApplicationLayer.DTO.MovieManagement;
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
    .           ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version.ToString()));

            //Update Movie
            CreateMap<MovieUpdateDto, Movie>()
                .ForMember(dest => dest.MovieGenres, opt => opt.Ignore())
                .ForMember(dest => dest.ShowTimes, opt => opt.Ignore())
                .ForMember(dest => dest.MovieImages, opt => opt.Ignore());

            //Search Movie
            CreateMap<Movie, MovieListDto>();

            //Genre
            CreateMap<Genre, GenreListDto>();
            CreateMap<GenreCreateDto, Genre>();

            //Cinema Room
            CreateMap<CinemaRoom, CinemaRoomListDto>();
        }
    }
}
