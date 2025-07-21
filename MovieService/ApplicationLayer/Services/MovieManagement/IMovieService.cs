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
        Task<IActionResult> GetByIdAsync(Guid movieId);
        Task<IActionResult> CreateMovie(MovieCreateDto Dto);
        Task<IActionResult> ViewMovie();
        Task<IActionResult> ViewMoviePagination(PaginationReq query);
        Task<IActionResult> UpdateMovie(MovieUpdateDto Dto);
        Task<IActionResult> DeleteMovie(Guid Id);
        Task<IActionResult> ChangeStatus(Guid Id, MovieStatus status);
        Task<IActionResult> SearchMovie(string? keyword);
        Task<IActionResult> GetAllGenre();
        Task<IActionResult> CreateGenre(GenreCreateDto Dto);
        Task<IActionResult> ChangeStatusGenre(Guid Id);
        Task<IActionResult> SetFeatured(Guid movieId, bool isFeatured);
        Task<IActionResult> SetRecommended(Guid movieId, bool isRecommended);
        Task<IActionResult> UpdateRating(Guid movieId, double rating);
        Task<IActionResult> GetRecommended();
        Task<IActionResult> GetComingSoon();
        Task<IActionResult> GetNowShowing();
    }
}
