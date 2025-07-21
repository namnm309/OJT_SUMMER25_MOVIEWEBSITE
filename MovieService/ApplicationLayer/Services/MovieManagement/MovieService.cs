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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationLayer.Services.MovieManagement
{
    public class MovieService : BaseService, IMovieService
    {
        private readonly IGenericRepository<Movie> _movieRepo;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<MovieGenre> _genreMovieRepo;
        private readonly IGenericRepository<Genre> _genreRepo;
        private readonly IGenericRepository<MovieImage> _imageRepo;
        private readonly IGenericRepository<ShowTime> _showtimeRepo;
        private readonly IGenericRepository<CinemaRoom> _roomRepo;

        private readonly IGenericRepository<Actor> _actorRepo;
        private readonly IGenericRepository<Director> _directorRepo;
        private readonly IGenericRepository<MovieActor> _movieActorRepo;
        private readonly IGenericRepository<MovieDirector> _movieDirectorRepo;

        private readonly IHttpContextAccessor _httpCtx;

        public MovieService(IGenericRepository<Movie> movieRepo, IGenericRepository<MovieGenre> genreMovieRepo, IGenericRepository<Genre> genreRepo, IGenericRepository<MovieImage> imageRepo, IGenericRepository<ShowTime> showtimeRepo, IGenericRepository<CinemaRoom> roomRepo, IGenericRepository<Actor> actorRepo, IGenericRepository<Director> directorRepo, IGenericRepository<MovieActor> movieActorRepo, IGenericRepository<MovieDirector> movieDirectorRepo, IMapper mapper, IHttpContextAccessor httpCtx) : base(mapper, httpCtx)
        {
            _movieRepo = movieRepo;
            _mapper = mapper;
            _httpCtx = httpCtx;
            _genreMovieRepo = genreMovieRepo;
            _genreRepo = genreRepo;
            _imageRepo = imageRepo;
            _showtimeRepo = showtimeRepo;
            _roomRepo = roomRepo;

            _actorRepo = actorRepo;
            _directorRepo = directorRepo;
            _movieActorRepo = movieActorRepo;
            _movieDirectorRepo = movieDirectorRepo;   
        }

        public async Task<List<MovieListDto>> GetAllAsync()
        {
            var movies = await _movieRepo.ListAsync();
            return _mapper.Map<List<MovieListDto>>(movies);
        }

        public async Task<IActionResult> GetByIdAsync(Guid movieId)
        {
            //var payload = ExtractPayload();
            //if (payload == null)
            //{
            //    return ErrorResp.Unauthorized("Invalid token");
            //}

            var movie = await _movieRepo.FirstOrDefaultAsync(
                m => m.Id == movieId,
                nameof(Movie.MovieImages),
                nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre),
                nameof(Movie.ShowTimes) + "." + nameof(ShowTime.Room)
                , nameof(Movie.MovieActors) + "." + nameof(MovieActor.Actor)
                , nameof(Movie.MovieDirectors) + "." + nameof(MovieDirector.Director)
            );
            
            if (movie == null) 
                return ErrorResp.NotFound("Movie not found");
            
            var result = _mapper.Map<MovieResponseDto>(movie);
            return SuccessResp.Ok(result);
        }

        // Phương thức GetByIdAsync cũ - hiện tại không sử dụng

        public async Task<IActionResult> CreateMovie(MovieCreateDto Dto)
        {
            

           

            var exist = await _movieRepo.FirstOrDefaultAsync(e => e.Title.ToLower() == Dto.Title.ToLower());
            if (exist != null)
                return ErrorResp.BadRequest("A movie with the same title already exists");

            if (Dto.ReleaseDate >= Dto.EndDate)
                return ErrorResp.BadRequest("Release date must be earlier than end date");

            if (Dto.GenreIds == null || !Dto.GenreIds.Any())
                return ErrorResp.BadRequest("At least one genre is required");

            // Bỏ validation bắt buộc lịch chiếu
            // if (Dto.ShowTimes == null || !Dto.ShowTimes.Any())
            //     return ErrorResp.BadRequest("At least one showtime is required");

            // Chỉ validate phòng chiếu khi có lịch chiếu
            if (Dto.ShowTimes != null && Dto.ShowTimes.Any())
            {
                foreach (var st in Dto.ShowTimes)
                {
                    var room = await _roomRepo.FindByIdAsync(st.RoomId);
                    if (room == null)
                        return ErrorResp.NotFound($"Cinema room with ID {st.RoomId} not found");
                }
            }

            if (Dto.Images == null || !Dto.Images.Any())
                return ErrorResp.BadRequest("At least one image is required");

            if (Dto.Images.Count(i => i.IsPrimary) != 1)
                return ErrorResp.BadRequest("Exactly one image must be marked as primary");

            // Validate actors & directors
            if (Dto.ActorIds == null || !Dto.ActorIds.Any())
                return ErrorResp.BadRequest("At least one actor is required");

            if (Dto.DirectorIds == null || !Dto.DirectorIds.Any())
                return ErrorResp.BadRequest("At least one director is required");

            foreach (var actorId in Dto.ActorIds)
            {
                var actor = await _actorRepo.FindByIdAsync(actorId);
                if (actor == null)
                    return ErrorResp.NotFound($"Actor with ID {actorId} not found");
            }

            foreach (var directorId in Dto.DirectorIds)
            {
                var director = await _directorRepo.FindByIdAsync(directorId);
                if (director == null)
                    return ErrorResp.NotFound($"Director with ID {directorId} not found");
            }

            foreach (var genreId in Dto.GenreIds)
            {
                var genre = await _genreRepo.FindByIdAsync(genreId);
                if (genre == null)
                    return ErrorResp.NotFound($"Genre with ID {genreId} not found");
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

            // Map diễn viên và đạo diễn
            var movieActors = Dto.ActorIds.Select(id => new MovieActor
            {
                MovieId = movie.Id,
                ActorId = id
            }).ToList();

            var movieDirectors = Dto.DirectorIds.Select(id => new MovieDirector
            {
                MovieId = movie.Id,
                DirectorId = id
            }).ToList();
 
            await _genreMovieRepo.CreateRangeAsync(movieGenres);
            await _imageRepo.CreateRangeAsync(movieImages);
            
            // Chỉ tạo lịch chiếu khi có dữ liệu
            if (Dto.ShowTimes != null && Dto.ShowTimes.Any())
            {
                var movieShowtimes = Dto.ShowTimes.Select(show => new ShowTime
                {
                    MovieId = movie.Id,
                    RoomId = show.RoomId,
                    ShowDate = show.ShowDate
                }).ToList();
                await _showtimeRepo.CreateRangeAsync(movieShowtimes);
            }

            await _movieActorRepo.CreateRangeAsync(movieActors);
            await _movieDirectorRepo.CreateRangeAsync(movieDirectors);

            return SuccessResp.Ok("Create movie successfully");
        }

        public async Task<IActionResult> ViewMovie()
        {
            var movies = await _movieRepo.ListAsync(
                nameof(Movie.MovieImages),
                nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
                , nameof(Movie.MovieActors) + "." + nameof(MovieActor.Actor)
                , nameof(Movie.MovieDirectors) + "." + nameof(MovieDirector.Director)
            );

            var result = _mapper.Map<List<MovieResponseDto>>(movies);

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> ViewMoviePagination(PaginationReq query)
        {
            int page = query.Page <= 0 ? 1 : query.Page;
            int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var movies = await _movieRepo.WhereAsync(
                filter: null,
                orderBy: q => q.OrderByDescending(m => m.CreatedAt),
                page: page - 1,
                pageSize: pageSize,
                navigationProperties: new[]
                {
                    nameof(Movie.MovieImages),
                    nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
                    , nameof(Movie.MovieActors) + "." + nameof(MovieActor.Actor)
                    , nameof(Movie.MovieDirectors) + "." + nameof(MovieDirector.Director)
                });

            var totalCount = await _movieRepo.CountAsync(); // tổng số phim

            var result = _mapper.Map<List<MovieResponseDto>>(movies);

            var response = new
            {
                Data = result,
                Total = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return SuccessResp.Ok(response);
        }

        public async Task<IActionResult> UpdateMovie(MovieUpdateDto Dto)
        {
            

            // Load movie với đầy đủ các quan hệ
            var movie = await _movieRepo.FirstOrDefaultAsync(
                m => m.Id == Dto.Id,
                nameof(Movie.MovieImages),
                nameof(Movie.MovieGenres),
                nameof(Movie.ShowTimes),
                nameof(Movie.MovieActors),
                nameof(Movie.MovieDirectors)
            );

            if(movie == null)
                return ErrorResp.NotFound("Movie Not Found");

            //Validate Movie (Id - Title)
            var duplicateTitle = await _movieRepo.FirstOrDefaultAsync(e =>e.Id != Dto.Id && e.Title.ToLower() == Dto.Title.ToLower());
            if (duplicateTitle != null)
                return ErrorResp.BadRequest("A movie with the same title already exists");

            // Validate ngày tháng
            if (Dto.ReleaseDate >= Dto.EndDate)
                return ErrorResp.BadRequest("Release date must be earlier than end date");

            if (Dto.GenreIds == null || !Dto.GenreIds.Any())
                return ErrorResp.BadRequest("At least one genre is required");

            if (Dto.Images.Count(i => i.IsPrimary) != 1)
                return ErrorResp.BadRequest("Exactly one image must be marked as primary");

            if (Dto.ActorIds == null || !Dto.ActorIds.Any())
                return ErrorResp.BadRequest("At least one actor is required");

            if (Dto.DirectorIds == null || !Dto.DirectorIds.Any())
                return ErrorResp.BadRequest("At least one director is required");

            // Validate references exist
            foreach (var actorId in Dto.ActorIds)
            {
                var actor = await _actorRepo.FindByIdAsync(actorId);
                if (actor == null)
                    return ErrorResp.NotFound($"Actor with ID {actorId} not found");
            }

            foreach (var directorId in Dto.DirectorIds)
            {
                var director = await _directorRepo.FindByIdAsync(directorId);
                if (director == null)
                    return ErrorResp.NotFound($"Director with ID {directorId} not found");
            }

            foreach (var genreId in Dto.GenreIds)
            {
                var genre = await _genreRepo.FindByIdAsync(genreId);
                if (genre == null)
                    return ErrorResp.NotFound($"Genre with ID {genreId} not found");
            }

            if (Dto.ShowTimes != null)
            {
                foreach (var st in Dto.ShowTimes)
                {
                    var room = await _roomRepo.FindByIdAsync(st.RoomId);
                    if (room == null)
                        return ErrorResp.NotFound($"Cinema room with ID {st.RoomId} not found");
                }
            }

            // Cập nhật thông tin cơ bản của phim
            _mapper.Map(Dto, movie);

            // Xóa các bản ghi cũ trong database
            await _genreMovieRepo.DeleteRangeAsync(movie.MovieGenres);
            await _movieActorRepo.DeleteRangeAsync(movie.MovieActors);
            await _movieDirectorRepo.DeleteRangeAsync(movie.MovieDirectors);
            await _imageRepo.DeleteRangeAsync(movie.MovieImages);
            if (movie.ShowTimes != null)
            {
                await _showtimeRepo.DeleteRangeAsync(movie.ShowTimes);
            }

            // Tạo các bản ghi mới
            var movieGenres = Dto.GenreIds.Select(gid => new MovieGenre
            {
                GenreId = gid,
                MovieId = movie.Id
            }).ToList();
            await _genreMovieRepo.CreateRangeAsync(movieGenres);
            movie.MovieGenres = movieGenres;

            var movieActors = Dto.ActorIds.Select(aid => new MovieActor
            {
                ActorId = aid,
                MovieId = movie.Id
            }).ToList();
            await _movieActorRepo.CreateRangeAsync(movieActors);
            movie.MovieActors = movieActors;

            var movieDirectors = Dto.DirectorIds.Select(did => new MovieDirector
            {
                DirectorId = did,
                MovieId = movie.Id
            }).ToList();
            await _movieDirectorRepo.CreateRangeAsync(movieDirectors);
            movie.MovieDirectors = movieDirectors;

            var movieImages = Dto.Images.Select(img => new MovieImage
            {
                ImageUrl = img.ImageUrl,
                Description = img.Description,
                DisplayOrder = img.DisplayOrder,
                IsPrimary = img.IsPrimary,
                MovieId = movie.Id
            }).ToList();
            await _imageRepo.CreateRangeAsync(movieImages);
            movie.MovieImages = movieImages;

            if (Dto.ShowTimes != null && Dto.ShowTimes.Any())
            {
                var movieShowtimes = Dto.ShowTimes.Select(st => new ShowTime
                {
                    RoomId = st.RoomId,
                    ShowDate = st.ShowDate,
                    MovieId = movie.Id
                }).ToList();
                await _showtimeRepo.CreateRangeAsync(movieShowtimes);
                movie.ShowTimes = movieShowtimes;
            }
            else
            {
                movie.ShowTimes = new List<ShowTime>();
            }

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

        public async Task<IActionResult> SearchMovie(string? keyword)
        {
            var movies = string.IsNullOrWhiteSpace(keyword)
                ? await _movieRepo.ListAsync(
                    nameof(Movie.MovieImages),
                    nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
                )
                : await _movieRepo.WhereAsync(
                    filter: m => m.Title.ToLower().Contains(keyword.ToLower()),
                    navigationProperties: new[]
                    {
                        nameof(Movie.MovieImages),
                        nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
                    });

            var result = _mapper.Map<List<MovieResponseDto>>(movies);

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetAllGenre()
        {
            var genre = await _genreRepo.ListAsync();

            var result = _mapper.Map<List<GenreListDto>>(genre);

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> CreateGenre(GenreCreateDto Dto)
        {
            

            var genre = await _genreRepo.FirstOrDefaultAsync(g => g.GenreName == Dto.GenreName);

            if (genre != null)
                return ErrorResp.BadRequest("The genre already exists");

            var result = _mapper.Map<Genre>(Dto);
            result.IsActive = true;

            await _genreRepo.CreateAsync(result);

            return SuccessResp.Ok("Create genre successfully");
        }

        public async Task<IActionResult> ChangeStatusGenre(Guid Id)
        {
            

            var genre = await _genreRepo.FindByIdAsync(Id);

            if (genre == null)
                return ErrorResp.NotFound("Genre Not Found");

            genre.IsActive = !genre.IsActive;
            genre.UpdatedAt = DateTime.UtcNow;

            await _genreRepo.UpdateAsync(genre);

            string status = genre.IsActive ? "Activated" : "De-Activated";
            return SuccessResp.Ok($"Genre has been {status} successfully");
        }

        public async Task<IActionResult> SetFeatured(Guid movieId, bool isFeatured)
        {
           

            var movie = await _movieRepo.FindByIdAsync(movieId);
            if (movie == null)
                return ErrorResp.NotFound("Movie not found");

            movie.IsFeatured = isFeatured;
            movie.UpdatedAt = DateTime.UtcNow;

            await _movieRepo.UpdateAsync(movie);

            string action = isFeatured ? "đã được đánh dấu là phim nổi bật" : "đã được bỏ khỏi danh sách phim nổi bật";
            return SuccessResp.Ok($"Phim {movie.Title} {action}");
        }

        public async Task<IActionResult> SetRecommended(Guid movieId, bool isRecommended)
        {
           

            var movie = await _movieRepo.FindByIdAsync(movieId);
            if (movie == null)
                return ErrorResp.NotFound("Movie not found");

            movie.IsRecommended = isRecommended;
            movie.UpdatedAt = DateTime.UtcNow;

            await _movieRepo.UpdateAsync(movie);

            string action = isRecommended ? "đã được đánh dấu là phim đề xuất" : "đã được bỏ khỏi danh sách phim đề xuất";
            return SuccessResp.Ok($"Phim {movie.Title} {action}");
        }

        public async Task<IActionResult> UpdateRating(Guid movieId, double rating)
        {
           

            var movie = await _movieRepo.FindByIdAsync(movieId);
            if (movie == null)
                return ErrorResp.NotFound("Movie not found");

            movie.Rating = rating;
            movie.UpdatedAt = DateTime.UtcNow;

            await _movieRepo.UpdateAsync(movie);

            return SuccessResp.Ok($"Đã cập nhật rating phim {movie.Title} thành {rating}/10");
        }

        public async Task<IActionResult> GetRecommended()
        {
            //var payload = ExtractPayload();
            //if (payload == null)
            //{
            //    return ErrorResp.Unauthorized("Invalid token");
            //}

            var movies = await _movieRepo.WhereAsync(
                filter: m => m.IsRecommended == true,
                orderBy: q => q.OrderByDescending(m => m.CreatedAt),
                navigationProperties: new[]
                {
                    nameof(Movie.MovieImages),
                    nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
                });

            var result = _mapper.Map<List<MovieResponseDto>>(movies);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetComingSoon()
        {
            //var payload = ExtractPayload();
            //if (payload == null)
            //{
            //    return ErrorResp.Unauthorized("Invalid token");
            //}

            var movies = await _movieRepo.WhereAsync(
                filter: m => m.Status == MovieStatus.ComingSoon,
                orderBy: q => q.OrderByDescending(m => m.ReleaseDate),
                navigationProperties: new[]
                {
                    nameof(Movie.MovieImages),
                    nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
                });

            var result = _mapper.Map<List<MovieResponseDto>>(movies);
            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetNowShowing()
        {
            //var payload = ExtractPayload();
            //if (payload == null)
            //{
            //    return ErrorResp.Unauthorized("Invalid token");
            //}

            var movies = await _movieRepo.WhereAsync(
                filter: m => m.Status == MovieStatus.NowShowing,
                orderBy: q => q.OrderByDescending(m => m.ReleaseDate),
                navigationProperties: new[]
                {
                    nameof(Movie.MovieImages),
                    nameof(Movie.MovieGenres) + "." + nameof(MovieGenre.Genre)
                });

            var result = _mapper.Map<List<MovieResponseDto>>(movies);
            return SuccessResp.Ok(result);
        }
    }
}
