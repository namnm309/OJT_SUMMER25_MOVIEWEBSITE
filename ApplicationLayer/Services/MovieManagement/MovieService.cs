using ApplicationLayer.DTO.MovieManagement;
using InfrastructureLayer.Repository;

namespace ApplicationLayer.Services.MovieManagement
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepository;
        public MovieService(IMovieRepository movieRepository)
        {
            _movieRepository = movieRepository;
        }

        // get full list of movies, mapping each Movie → MovieListDto
        public async Task<List<MovieListDto>> GetAllAsync()
        {
            var movies = await _movieRepository.GetAllMoviesAsync();
            return movies.Select(m => new MovieListDto
            {
                MovieId = m.MovieId,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate,
                ProductionCompany = m.ProductionCompany,
                RunningTime = m.RunningTime,
                Version = m.Version
            }).ToList();
        }

        // get movies by id
        public async Task<MovieListDto?> GetByIdAsync(Guid id)
        {
            var movie = await _movieRepository.GetMovieByIdAsync(id);
            if (movie == null) return null;
            return new MovieListDto
            {
                MovieId = movie.MovieId,
                Title = movie.Title,
                ReleaseDate = movie.ReleaseDate,
                ProductionCompany = movie.ProductionCompany,
                RunningTime = movie.RunningTime,
                Version = movie.Version
            };
        }
    }
}
