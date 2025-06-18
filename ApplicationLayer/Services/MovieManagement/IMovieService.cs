using ApplicationLayer.DTO;
using ApplicationLayer.DTO.MovieManagement;
using DomainLayer.Enum;
using Microsoft.AspNetCore.Mvc;
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
        Task<MovieResponseDto?> GetByIdAsync(Guid movieId);
        Task<IActionResult> CreateMovie(MovieCreateDto Dto);
        Task<IActionResult> ViewMovie();
        Task<IActionResult> ViewMoviesWithPagination(PaginationReq query);
        Task<IActionResult> UpdateMovie(MovieUpdateDto Dto);
        Task<IActionResult> DeleteMovie(Guid Id);
        Task<IActionResult> ChangeStatus(Guid Id, MovieStatus status);
        
        // New methods for getting genres and cinema rooms
        Task<IActionResult> GetAllGenres();
        Task<IActionResult> GetAllCinemaRooms();
    }
}
