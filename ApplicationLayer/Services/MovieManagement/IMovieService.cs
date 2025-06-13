using ApplicationLayer.DTO.MovieManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.MovieManagement
{
    public interface IMovieService
    {
        Task<List<MovieListDto>> GetAllAsync();
        Task<MovieListDto?> GetByIdAsync(Guid movieId);
    }
}
