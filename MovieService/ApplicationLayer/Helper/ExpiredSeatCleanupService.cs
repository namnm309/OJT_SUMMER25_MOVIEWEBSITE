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
                    var seatRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Seat>>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<SeatHub>>();

                    // Truy vấn các SeatLog đã hết hạn và include Seat
                    var expiredLogs = await seatLogRepo.WhereAsync(
                        log => log.ExpiredAt < DateTime.UtcNow,
                        "Seat"
                    );

                    // Group theo ShowTimeId để gửi SignalR hợp lý
                    var groupedLogs = expiredLogs
                        .GroupBy(log => log.ShowTimeId)
                        .ToList();

                    foreach (var group in groupedLogs)
                    {
                        var seatIds = group.Select(log => log.SeatId).ToList();

                        // Update trạng thái ghế
                        var seats = await seatRepo.WhereAsync(s => seatIds.Contains(s.Id));
                        foreach (var seat in seats)
                        {
                            seat.Status = SeatStatus.Available;
                        }
                        await seatRepo.UpdateRangeAsync(seats);

                        // Xoá log
                        foreach (var log in group)
                        {
                            await seatLogRepo.DeleteAsync(log);
                        }

                        // Gửi thông báo SignalR theo từng Showtime
                        await hubContext.Clients.Group(group.Key.ToString())
                            .SendAsync("SeatsReleased", seatIds);
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
