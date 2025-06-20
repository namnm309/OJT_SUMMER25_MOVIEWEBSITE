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
            return await dbSet.ToListAsync();
        }

        // retrieve a movie by its ID
        public async Task<Movie?> GetMovieByIdAsync(Guid id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<List<Movie>> SearchMoviesByNameAsync(string? keyword)
        {
            // start with the full set
            IQueryable<Movie> q = dbSet;

            // if keyword provided, filter on Title
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                q = q.Where(m => EF.Functions.ILike(m.Title, $"%{k}%"));
            }

            // sort A→Z
            q = q.OrderBy(m => m.Title);

            return await q.ToListAsync();
        }

    }
}
