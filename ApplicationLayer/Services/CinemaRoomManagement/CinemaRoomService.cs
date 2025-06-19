using Application.ResponseCode;
using ApplicationLayer.DTO;
using ApplicationLayer.DTO.CinemaRoomManagement;
using AutoMapper;
using DomainLayer.Entities;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.CinemaRoomManagement
{
    public interface ICinemaRoomService
    {
        public Task<IActionResult> GetAllCinemaRoom();
        public Task<IActionResult> GetAllCinemaRoomPagination(PaginationReq query);
        public Task<IActionResult> GetSeatDetail(Guid Id);
        public Task<IActionResult> AddCinemaRoom(CinemaRoomCreateDto Dto);
        public Task<IActionResult> SearchCinemaRoom(string? keyword);

    }
    
    public class CinemaRoomService : ICinemaRoomService
    {
        private readonly IGenericRepository<CinemaRoom> _cinemaRoomRepo;
        private readonly IMapper _mapper;

        public CinemaRoomService(IGenericRepository<CinemaRoom> cinemaRoomRepo, IMapper mapper)
        {
            _cinemaRoomRepo = cinemaRoomRepo;
            _mapper = mapper;
        }

        public async Task<IActionResult> GetAllCinemaRoom()
        {
            var room = await _cinemaRoomRepo.ListAsync();

            var result = _mapper.Map<List<CinemaRoomListDto>>(room);

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetAllCinemaRoomPagination(PaginationReq query)
        {
            int page = query.Page <= 0 ? 1 : query.Page;
            int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var room = await _cinemaRoomRepo.ListAsync();

            var pagedRoom = room
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Data = pagedRoom,
                Total = room.Count,
                Page = query.Page,
                PageSize = query.PageSize,
            };

            return SuccessResp.Ok(response);
        }

        public async Task<IActionResult> GetSeatDetail(Guid Id)
        {
            var room = await _cinemaRoomRepo.FoundOrThrowAsync(Id, "Room not found", "Seats");

            var seat = _mapper.Map<List<SeatDetailDto>>(room.Seats);

            return SuccessResp.Ok(seat);
        }

        public async Task<IActionResult> AddCinemaRoom(CinemaRoomCreateDto Dto)
        {
            var exist = await _cinemaRoomRepo.FirstOrDefaultAsync(r => r.RoomName.Trim().ToLower() == Dto.RoomName.Trim().ToLower());

            if (exist != null)
                return ErrorResp.BadRequest("Cinema room with this name already exists");

            var room = _mapper.Map<CinemaRoom>(Dto);

            await _cinemaRoomRepo.CreateAsync(room);

            return SuccessResp.Ok("Add CinemaRoom Successfully");
        }

        public async Task<IActionResult> SearchCinemaRoom(string? keyword)
        {
            var room = string.IsNullOrWhiteSpace(keyword)
                ? await _cinemaRoomRepo.ListAsync()
                : await _cinemaRoomRepo.WhereAsync(r => r.RoomName.Contains(keyword));

            var result = _mapper.Map<List<CinemaRoomListDto>>(room);

            return SuccessResp.Ok(result);
        }
    }
}
