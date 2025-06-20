using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.ResponseCode;
using ApplicationLayer.DTO;
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
        private readonly IGenericRepository<Genre> _genreEntityRepo;
        //added new repo for search functionality
        private readonly IMovieRepository _movieRepository;

        public MovieService(IGenericRepository<Movie> movieRepo, IGenericRepository<MovieGenre> genreRepo, IGenericRepository<MovieImage> imageRepo, IGenericRepository<ShowTime> showtimeRepo, IGenericRepository<CinemaRoom> roomRepo, IGenericRepository<Genre> genreEntityRepo, IMapper mapper)
        {
            _movieRepo = movieRepo;
            _mapper = mapper;
            _genreRepo = genreRepo;
            _imageRepo = imageRepo;
            _showtimeRepo = showtimeRepo;
            _roomRepo = roomRepo;
            _genreEntityRepo = genreEntityRepo;
        }

        public async Task<List<MovieListDto>> GetAllAsync()
        {
            var movies = await _movieRepo.ListAsync();
            return _mapper.Map<List<MovieListDto>>(movies);
        }

        public async Task<MovieResponseDto?> GetByIdAsync(Guid movieId)
        {
            var movie = await _movieRepo.FirstOrDefaultAsync(
                m => m.Id == movieId,
                "MovieImages", "MovieGenres.Genre"
            );
            
            if (movie == null) return null;
            
            return new MovieResponseDto
            {
                Id = movie.Id,
                Title = movie.Title,
                ReleaseDate = movie.ReleaseDate ?? DateTime.Now,
                ProductionCompany = movie.ProductionCompany,
                RunningTime = movie.RunningTime,
                Version = movie.Version?.ToString() ?? "TwoD",
                Director = movie.Director,
                Actors = movie.Actors,
                Content = movie.Content,
                TrailerUrl = movie.TrailerUrl,
                Status = (int)movie.Status,
                // Lấy hình ảnh primary
                PrimaryImageUrl = movie.MovieImages?
                    .FirstOrDefault(img => img.IsPrimary)?.ImageUrl,
                // Lấy tất cả hình ảnh
                Images = movie.MovieImages?
                    .Select(img => new MovieImageDto
                    {
                        ImageUrl = img.ImageUrl,
                        Description = img.Description ?? "",
                        DisplayOrder = img.DisplayOrder,
                        IsPrimary = img.IsPrimary
                    }).ToList() ?? new List<MovieImageDto>(),
                // Lấy danh sách thể loại
                Genres = movie.MovieGenres?
                    .Select(mg => mg.Genre?.GenreName ?? "")
                    .Where(g => !string.IsNullOrEmpty(g))
                    .ToList() ?? new List<string>()
            };
        }

        // Search method (movie title)
        public async Task<List<MovieListDto>> SearchAsync(string? keyword)
        {
            var movies = await _movieRepository.GetMovieByKeywordAsync(keyword);
            // map to DTO
            return movies
                .Select(m => new MovieListDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    ReleaseDate = m.ReleaseDate,
                    ProductionCompany = m.ProductionCompany,
                    RunningTime = m.RunningTime.ToString(),
                    Version =  m.Version
                })
                .ToList();
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

            var movie = _mapper.Map<Movie>(Dto);
            movie.Status = MovieStatus.NotAvailable;

            await _movieRepo.CreateAsync(movie);

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
            var movies = await _movieRepo.ListAsync(
                "MovieImages", "MovieGenres.Genre"
            );
            
            var result = movies.Select(movie => new MovieResponseDto
            {
                Id = movie.Id,
                Title = movie.Title,
                ReleaseDate = movie.ReleaseDate ?? DateTime.Now,
                ProductionCompany = movie.ProductionCompany,
                RunningTime = movie.RunningTime,
                Version = movie.Version?.ToString() ?? "TwoD",
                Director = movie.Director,
                Actors = movie.Actors,
                Content = movie.Content,
                TrailerUrl = movie.TrailerUrl,
                Status = (int)movie.Status,
                // Lấy hình ảnh primary
                PrimaryImageUrl = movie.MovieImages?
                    .FirstOrDefault(img => img.IsPrimary)?.ImageUrl,
                // Lấy tất cả hình ảnh
                Images = movie.MovieImages?
                    .Select(img => new MovieImageDto
                    {
                        ImageUrl = img.ImageUrl,
                        Description = img.Description ?? "",
                        DisplayOrder = img.DisplayOrder,
                        IsPrimary = img.IsPrimary
                    }).ToList() ?? new List<MovieImageDto>(),
                // Lấy danh sách thể loại
                Genres = movie.MovieGenres?
                    .Select(mg => mg.Genre?.GenreName ?? "")
                    .Where(g => !string.IsNullOrEmpty(g))
                    .ToList() ?? new List<string>()
            }).ToList();
            
            return SuccessResp.Ok(result);
        }

        //Code movie with pagination 
        public async Task<IActionResult> ViewMoviesWithPagination(PaginationReq query)
        {
            int page = query.Page <= 0 ? 1 : query.Page;
            int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var movies = await _movieRepo.ListAsync();

            var pagedMovies = movies
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Data = pagedMovies,
                Total = movies.Count,
                Page = query.Page,
                PageSize = query.PageSize,
            };

            return SuccessResp.Ok(response);
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

            _mapper.Map(Dto, movie);

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

        public async Task<IActionResult> GetAllGenres()
        {
            var genres = await _genreEntityRepo.ListAsync();
            var genreDtos = genres.Select(g => new GenreDto
            {
                Id = g.Id,
                Name = g.GenreName,
                Description = g.Description
            }).ToList();

            return SuccessResp.Ok(genreDtos);
        }

        public async Task<IActionResult> GetAllCinemaRooms()
        {
            var rooms = await _roomRepo.ListAsync();
            var roomDtos = rooms.Select(r => new CinemaRoomDto
            {
                Id = r.Id,
                RoomName = r.RoomName,
                TotalSeats = r.TotalSeats,
                IsActive = r.IsActive
            }).ToList();

            return SuccessResp.Ok(roomDtos);
        }


    }
}
