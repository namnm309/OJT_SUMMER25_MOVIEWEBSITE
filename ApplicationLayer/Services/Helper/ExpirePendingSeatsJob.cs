using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.Helper
{ 
    public class ExpirePendingSeatsJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ExpirePendingSeatsJob(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var seatRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Seat>>();
                    var seatLogRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<SeatLog>>();
                    var bookingRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Booking>>();

                    // Lấy danh sách log ghế đã hết hạn
                    var expiredLogs = await seatLogRepo.WhereAsync(s => s.Status == SeatStatus.Pending && s.ExpiredAt <= DateTime.UtcNow);
                    if (expiredLogs.Any())
                    {
                        // Cập nhật Status ghế về Available
                        var seadIds = expiredLogs.Select(x => x.SeatId).Distinct().ToList();
                        var seats = await seatRepo.WhereAsync(s => seadIds.Contains(s.Id));
                        foreach (var seat in seats)
                        {
                            seat.Status = SeatStatus.Available;
                        }
                        await seatRepo.UpdateRangeAsync(seats);

                        // Cập nhật trạng thái log
                        foreach (var log in expiredLogs)
                        {
                            log.Status = SeatStatus.Available;
                        }
                        await seatLogRepo.UpdateRangeAsync(expiredLogs);

                        // Hủy booking nếu vẫn Pending
                        var bookingIds = expiredLogs.Select(x => x.BookingId).Distinct().ToList();
                        var expiredBookings = await bookingRepo.WhereAsync(b => bookingIds.Contains(b.Id) && b.Status == BookingStatus.Pending);

                        foreach (var booking in expiredBookings)
                        {
                            booking.Status = BookingStatus.Canceled;
                        }
                        await bookingRepo.UpdateRangeAsync(expiredBookings);
                    }
                    // Delay 1 phút rồi chạy tiếp
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task canceled, shutting down background job...");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                }
            }
        }
    }
}
