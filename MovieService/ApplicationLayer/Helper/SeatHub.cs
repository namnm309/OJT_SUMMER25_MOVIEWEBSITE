using ApplicationLayer.Helper;
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
    public override async Task OnConnectedAsync()
    {
        var showTimeId = Context.GetHttpContext()?.Request.Query["showTimeId"];
        if (!string.IsNullOrEmpty(showTimeId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, showTimeId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var showTimeId = Context.GetHttpContext()?.Request.Query["showTimeId"];
        if (!string.IsNullOrEmpty(showTimeId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, showTimeId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}
