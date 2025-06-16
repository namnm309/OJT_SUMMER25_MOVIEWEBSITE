using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.ResponseCode;
using ApplicationLayer.DTO.MovieManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationLayer.Services.MovieManagement
{
    public class MovieService : IMovieService
    {
        private readonly IGenericRepository<Movie> _movieRepo;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<MovieGenre> _genreRepo;
        private readonly IGenericRepository<MovieImage> _imageRepo;
        private readonly IGenericRepository<ShowTime> _showtimeRepo;
        private readonly IGenericRepository<CinemaRoom> _roomRepo;

        public MovieService(IGenericRepository<Movie> movieRepo, IGenericRepository<MovieGenre> genreRepo, IGenericRepository<MovieImage> imageRepo, IGenericRepository<ShowTime> showtimeRepo, IGenericRepository<CinemaRoom> roomRepo, IMapper mapper)
        {
            _movieRepo = movieRepo;
            _mapper = mapper;
            _genreRepo = genreRepo;
            _imageRepo = imageRepo;
            _showtimeRepo = showtimeRepo;
            _roomRepo = roomRepo;
        }

        public async Task<List<MovieListDto>> GetAllAsync()
        {
            var movies = await _movieRepo.ListAsync();
            return _mapper.Map<List<MovieListDto>>(movies);
        }

        public async Task<MovieListDto?> GetByIdAsync(Guid movieId)
        {
            var movie = await _movieRepo.FindByIdAsync(movieId);
            return movie == null ? null : _mapper.Map<MovieListDto>(movie);
        }

        public async Task<IActionResult> CreateMovie(MovieCreateDto Dto)
        {
            var exist = await _movieRepo.FirstOrDefaultAsync(e => e.Title.ToLower() == Dto.Title.ToLower());
            if (exist != null)
                return ErrorResp.BadRequest("A movie with the same title already exists");

            if (Dto.ReleaseDate >= Dto.EndDate)
                return ErrorResp.BadRequest("Release date must be earlier than end date");

            if (Dto.GenreIds == null || !Dto.GenreIds.Any())
                return ErrorResp.BadRequest("At least one genre is required");

            if (Dto.ShowTimes == null || !Dto.ShowTimes.Any())
                return ErrorResp.BadRequest("At least one showtime is required");

            if (Dto.Images == null || !Dto.Images.Any())
                return ErrorResp.BadRequest("At least one image is required");

            if (Dto.Images.Count(i => i.IsPrimary) != 1)
                return ErrorResp.BadRequest("Exactly one image must be marked as primary");


            foreach (var genreId in Dto.GenreIds)
            {
                var genre = await _genreRepo.FindByIdAsync(genreId);
                if (genre == null)
                    return ErrorResp.NotFound($"Genre with ID {genreId} not found");
            }

            foreach (var st in Dto.ShowTimes)
            {
                var room = await _roomRepo.FindByIdAsync(st.RoomId);
                if (room == null)
                    return ErrorResp.NotFound($"Cinema room with ID {st.RoomId} not found");
            }

            var movie = _mapper.Map<Movie>(Dto);
            movie.Status = MovieStatus.NotAvailable;

            await _movieRepo.CreateAsync(movie);

            // Map danh sách thể loại (GenreIds -> MovieGenres)
            var movieGenres = Dto.GenreIds.Select(id => new MovieGenre
            {
                MovieId = movie.Id,
                GenreId = id
            }).ToList();

            // Map hình ảnh
            var movieImages = _mapper.Map<List<MovieImage>>(Dto.Images)
                .Select(image =>
                {
                    image.MovieId = movie.Id;
                    return image;
                }).ToList();

            // Map suất chiếu
            var showtimes = Dto.ShowTimes.Select(show => new ShowTime
            {
                MovieId = movie.Id,
                RoomId = show.RoomId,
                ShowDate = show.ShowDate
            }).ToList();

            await _genreRepo.CreateRangeAsync(movieGenres);
            await _imageRepo.CreateRangeAsync(movieImages);
            await _showtimeRepo.CreateRangeAsync(showtimes);

            return SuccessResp.Ok("Create movie successfully");
        }

        public async Task<IActionResult> ViewMovie()
        {
            var movies = await _movieRepo.ListAsync();
            var result = _mapper.Map<List<MovieResponseDto>>(movies);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> UpdateMovie(MovieUpdateDto Dto)
        {
            var movie = await _movieRepo.FindByIdAsync(Dto.Id);
            if(movie == null)
                return ErrorResp.NotFound("Movie Not Found");

            //Valitdate Movie (Id - Title)
            var duplicateTitle = await _movieRepo.FirstOrDefaultAsync(e =>e.Id != Dto.Id && e.Title.ToLower() == Dto.Title.ToLower());
            if (duplicateTitle != null)
                return ErrorResp.BadRequest("A movie with the same title already exists");

            // Validate ngày tháng
            if (Dto.ReleaseDate >= Dto.EndDate)
                return ErrorResp.BadRequest("Release date must be earlier than end date");

            if (Dto.GenreIds == null || !Dto.GenreIds.Any())
                return ErrorResp.BadRequest("At least one genre is required");

            if (Dto.ShowTimes == null || !Dto.ShowTimes.Any())
                return ErrorResp.BadRequest("At least one showtime is required");

            if (Dto.Images.Count(i => i.IsPrimary) != 1)
                return ErrorResp.BadRequest("Exactly one image must be marked as primary");

            foreach (var genreId in Dto.GenreIds)
            {
                var genre = await _genreRepo.FindByIdAsync(genreId);
                if (genre == null)
                    return ErrorResp.NotFound($"Genre with ID {genreId} not found");
            }

            foreach (var st in Dto.ShowTimes)
            {
                var room = await _roomRepo.FindByIdAsync(st.RoomId);
                if (room == null)
                    return ErrorResp.NotFound($"Cinema room with ID {st.RoomId} not found");
            }

            _mapper.Map(Dto, movie);

            // Cập nhật Genres
            movie.MovieGenres.Clear();
            movie.MovieGenres = Dto.GenreIds.Select(gid => new MovieGenre
            {
                GenreId = gid,
                MovieId = movie.Id
            }).ToList();

            // Cập nhật ShowTimes
            movie.ShowTimes.Clear();
            movie.ShowTimes = Dto.ShowTimes.Select(st => new ShowTime
            {
                RoomId = st.RoomId,
                ShowDate = st.ShowDate,
                MovieId = movie.Id
            }).ToList();

            // Cập nhật MovieImages
            movie.MovieImages.Clear();
            movie.MovieImages = Dto.Images.Select(img => new MovieImage
            {
                ImageUrl = img.ImageUrl,
                Description = img.Description,
                DisplayOrder = img.DisplayOrder,
                IsPrimary = img.IsPrimary,
                MovieId = movie.Id
            }).ToList();

            await _movieRepo.UpdateAsync(movie);
            return SuccessResp.Ok("Movie updated successfully");
        }

        public async Task<IActionResult> DeleteMovie(Guid Id)
        {
            var movie = await _movieRepo.FindByIdAsync(Id);
            if (movie == null)
                return ErrorResp.NotFound("Movie Not Found");

            movie.Status = MovieStatus.Stopped;
            await _movieRepo.UpdateAsync(movie);

            return SuccessResp.Ok("Movie Deleted Successfully");
        }

        public async Task<IActionResult> ChangeStatus(Guid Id, MovieStatus status)
        {
            var movie = await _movieRepo.FindByIdAsync(Id);
            if (movie == null)
                return ErrorResp.NotFound("Movie Not Found");

            movie.Status = status;
            await _movieRepo.UpdateAsync(movie);

            return SuccessResp.Ok("Changed Status Successfully");
        }
    }
}
