using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.Helper;
using ApplicationLayer.Services.BookingTicketManagement;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.SignalR;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SeatHub : Hub
{
    private readonly ISeatSignalService _seatSignalService;

    public SeatHub(ISeatSignalService seatSignalService)
    {
        _seatSignalService = seatSignalService;
    }

    private Guid? GetUserIdFromToken()
    {
        var claim = Context.User?.Claims.FirstOrDefault(c => c.Type == "UserId");
        if (claim != null && Guid.TryParse(claim.Value, out var userId))
            return userId;

        return null;
    }

    public async Task HoldSeat(HoldSeatSignalRequest request)
    {
        var result = await _seatSignalService.HoldSeatAsync(request);
        if (!result.Success)
        {
            await Clients.Caller.SendAsync("HoldFailed", result.Message);
            return;
        }

        var userId = GetUserIdFromToken();

        await Clients.Others.SendAsync("SeatHeld", new
        {
            request.ShowTimeId,
            request.SeatIds,
            UserId = userId,
            result.ExpiredAt
        });

        await Clients.Caller.SendAsync("HoldSuccess", new
        {
            request.SeatIds,
            result.ExpiredAt
        });
    }

    public async Task ReleaseSeat(ReleaseSeatSignalRequest request)
    {
        await _seatSignalService.ReleaseSeatAsync(request);

        await Clients.All.SendAsync("SeatReleased", new
        {
            request.ShowTimeId,
            request.SeatIds
        });
    }
}
