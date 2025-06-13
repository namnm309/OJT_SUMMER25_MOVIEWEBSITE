using DomainLayer.Entities;
using InfrastructureLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repository
{
    public class MovieRepository : GenericRepository<Movie>, IMovieRepository
    {
        // MovieRepository kế thừa GenericRepository để có sẵn các methods cơ bản
        // + implement các methods đặc biệt cho Movie
        public MovieRepository(MovieContext context) : base(context)
        {
        }

        // retrieve all movies from the database
        public async Task<List<Movie>> GetAllMoviesAsync()
        {
            return await _dbSet.ToListAsync();
        }

        // retrieve a movie by its ID
        public async Task<Movie?> GetMovieByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
}
