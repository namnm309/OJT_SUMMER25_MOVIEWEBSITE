using DomainLayer.Entities;

namespace InfrastructureLayer.Repository
{
    public interface IMovieRepository
    {
        // MovieRepository kế thừa GenericRepository để có sẵn các methods cơ bản
        // + implement các methods đặc biệt cho Movie
        Task<List<Movie>> GetAllMoviesAsync();  // retrieve all movies from the database

        Task<Movie?> GetMovieByIdAsync(Guid id); // retrieve a movie by its ID
    }
}
