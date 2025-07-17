using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Helper
{
    public class ExpiredSeatCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredSeatCleanupService> _logger;

        public ExpiredSeatCleanupService(IServiceProvider serviceProvider, ILogger<ExpiredSeatCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var seatLogRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<SeatLog>>();
                    var seatLogDetailRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<SeatLogDetail>>();
                    var seatRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Seat>>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<SeatHub>>();

                    var expiredLogs = await seatLogRepo.WhereAsync(
                        log => log.ExpiredAt < DateTime.UtcNow,
                        "SeatLogDetails"
                    );

                    foreach (var log in expiredLogs)
                    {
                        var expiredSeatIds = log.SeatLogDetails.Select(d => d.SeatId).ToList();

                        // Cập nhật trạng thái ghế
                        var seatsToUpdate = await seatRepo.WhereAsync(s => expiredSeatIds.Contains(s.Id));
                        foreach (var seat in seatsToUpdate)
                        {
                            seat.Status = SeatStatus.Available;
                        }
                        await seatRepo.UpdateRangeAsync(seatsToUpdate);

                        // Xóa log detail + log
                        await seatLogDetailRepo.DeleteRangeAsync(log.SeatLogDetails);
                        await seatLogRepo.DeleteAsync(log);

                        // Gửi thông báo SignalR
                        await hubContext.Clients.Group(log.ShowTimeId.ToString())
                            .SendAsync("SeatsReleased", expiredSeatIds);
                    }

                    _logger.LogInformation("✔ Cleared {Count} expired seat logs at {Time}", expiredLogs.Count, DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during expired seat cleanup");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // kiểm tra mỗi 1 phút
            }
        }
    }
}
