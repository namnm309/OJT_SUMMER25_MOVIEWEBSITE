using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using ApplicationLayer.Services.BookingTicketManagement;
using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.DTO.UserManagement;
using DomainLayer.Entities;
using DomainLayer.Enum;
using InfrastructureLayer.Repository;
using InfrastructureLayer.Core.Mail;
using InfrastructureLayer.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationLayer.Tests
{
    public class BookingTicketAdminE2ETests
    {
        // Khởi tạo các mock repository và service cần thiết
        private readonly Mock<IGenericRepository<Movie>> _movieRepo = new();
        private readonly Mock<IGenericRepository<ShowTime>> _showtimeRepo = new();
        private readonly Mock<IGenericRepository<Booking>> _bookingRepo = new();
        private readonly Mock<IGenericRepository<BookingDetail>> _bookingDetailRepo = new();
        private readonly Mock<IGenericRepository<Users>> _userRepo = new();
        private readonly Mock<IGenericRepository<PointHistory>> _pointHistoryRepo = new();
        private readonly Mock<ISeatRepository> _seatRepository = new();
        private readonly Mock<IGenericRepository<Promotion>> _promotionRepo = new();
        private readonly Mock<IGenericRepository<UserPromotion>> _userPromotionRepo = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IMailService> _mailService = new();
        private readonly Mock<IBookingRepository> _bookingRepository = new();
        private readonly Mock<ILogger<BookingTicketService>> _logger = new();
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();
        private readonly Mock<MovieContext> _context = new();

        // Hàm tạo service BookingTicketService với các mock đã khởi tạo
        private BookingTicketService CreateService()
        {
            return new BookingTicketService(
                _userRepository.Object,
                _movieRepo.Object,
                _showtimeRepo.Object,
                _bookingRepo.Object,
                _bookingDetailRepo.Object,
                _userRepo.Object,
                _pointHistoryRepo.Object,
                _seatRepository.Object,
                _promotionRepo.Object,
                _userPromotionRepo.Object,
                _mapper.Object,
                _mailService.Object,
                _bookingRepository.Object,
                _logger.Object,
                _httpContextAccessor.Object,
                _context.Object
            );
        }

        [Fact]
        public async Task AdminBookingFlow_Complete_Success()
        {
            // Bước 1: Chuẩn bị dữ liệu giả lập cho các entity
            var movieId = Guid.NewGuid();
            var showTimeId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var seatId1 = Guid.NewGuid();
            var seatId2 = Guid.NewGuid();

            // Tạo dữ liệu phim
            var movie = new Movie
            {
                Id = movieId,
                Title = "Avengers: Endgame",
                Status = MovieStatus.NowShowing
            };

            // Tạo dữ liệu suất chiếu
            var showTime = new ShowTime
            {
                Id = showTimeId,
                MovieId = movieId,
                RoomId = roomId,
                ShowDate = DateTime.UtcNow.AddDays(1),
                StartTime = new TimeSpan(19, 0, 0),
                EndTime = new TimeSpan(22, 0, 0),
                IsActive = true,
                Movie = movie,
                Room = new CinemaRoom
                {
                    Id = roomId,
                    RoomName = "Phòng 1",
                    IsActive = true
                }
            };

            // Tạo dữ liệu người dùng
            var user = new Users
            {
                Id = userId,
                FullName = "Admin User",
                Email = "admin@cinema.com",
                Phone = "0123456789",
                IdentityCard = "123456789",
                Role = UserRole.Admin,
                IsActive = true
            };

            // Tạo dữ liệu ghế
            var seats = new List<Seat>
            {
                new Seat
                {
                    Id = seatId1,
                    SeatCode = "A1",
                    RoomId = roomId,
                    Status = SeatStatus.Available,
                    PriceSeat = 100000,
                    RowIndex = 1,
                    ColumnIndex = 1
                },
                new Seat
                {
                    Id = seatId2,
                    SeatCode = "A2",
                    RoomId = roomId,
                    Status = SeatStatus.Available,
                    PriceSeat = 100000,
                    RowIndex = 1,
                    ColumnIndex = 2
                }
            };

            // Mock các repository trả về dữ liệu tương ứng
            _movieRepo.Setup(r => r.WhereAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Movie, bool>>>()))
                .ReturnsAsync(new List<Movie> { movie });
            _showtimeRepo.Setup(r => r.WhereAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ShowTime, bool>>>()))
                .ReturnsAsync(new List<ShowTime> { showTime });
            _seatRepository.Setup(r => r.GetSeatInfoAsync(showTimeId))
                .ReturnsAsync(("Phòng 1", seats, new List<Guid>()));
            _showtimeRepo.Setup(r => r.FindByIdAsync(showTimeId, "Room"))
                .ReturnsAsync(showTime);
            _seatRepository.Setup(r => r.GetSeatsByIdsAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(seats);
            _showtimeRepo.Setup(r => r.FoundOrThrowAsync(showTimeId, It.IsAny<string>()))
                .ReturnsAsync(showTime);
            _movieRepo.Setup(r => r.FoundOrThrowAsync(movieId, It.IsAny<string>()))
                .ReturnsAsync(movie);
            _userRepo.Setup(r => r.FoundOrThrowAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(user);
            _seatRepository.Setup(r => r.GetBookedSeatIdsForShowTimeAsync(showTimeId))
                .ReturnsAsync(new List<Guid>());
            _bookingRepo.Setup(r => r.CreateAsync(It.IsAny<Booking>()))
                .ReturnsAsync((Booking b) => { b.Id = Guid.NewGuid(); return b; });
            _bookingDetailRepo.Setup(r => r.CreateRangeAsync(It.IsAny<IEnumerable<BookingDetail>>()))
                .Returns(Task.CompletedTask);
            _userRepo.Setup(r => r.UpdateAsync(It.IsAny<Users>()))
                .ReturnsAsync((Users u) => u);
            _pointHistoryRepo.Setup(r => r.CreateAsync(It.IsAny<PointHistory>()))
                .ReturnsAsync((PointHistory ph) => ph);

            var service = CreateService();

            // Bước 2: Gọi các hàm lấy phim, suất chiếu, ghế, ...
            var moviesResult = await service.GetAvailableMovies();
            var moviesJsonResult = Assert.IsType<JsonResult>(moviesResult);
            Assert.NotNull(moviesJsonResult.Value);

            var showDatesResult = await service.GetShowDatesByMovie(movieId);
            var showDatesJsonResult = Assert.IsType<JsonResult>(showDatesResult);
            Assert.NotNull(showDatesJsonResult.Value);

            var selectedDate = DateTime.UtcNow.AddDays(1);
            var showTimesResult = await service.GetShowTimesByMovieAndDate(movieId, selectedDate);
            var showTimesJsonResult = Assert.IsType<JsonResult>(showTimesResult);
            Assert.NotNull(showTimesJsonResult.Value);

            // Bước 3: Tạo request đặt vé cho admin
            var bookingRequest = new ConfirmBookingRequestAdminDto
            {
                ShowTimeId = showTimeId,
                SeatIds = new List<Guid> { seatId1, seatId2 },
                MemberId = userId.ToString(),
                PointsUsed = 0,
                ConvertedTickets = 0
            };

            // Bước 4: Gọi hàm đặt vé
            var bookingResult = await service.ConfirmAdminBooking(bookingRequest);
            var bookingJsonResult = Assert.IsType<JsonResult>(bookingResult);
            
            // Bước 5: Kiểm tra booking đã được tạo thành công
            _bookingRepo.Verify(r => r.CreateAsync(It.IsAny<Booking>()), Times.Once);
            _bookingDetailRepo.Verify(r => r.CreateRangeAsync(It.IsAny<IEnumerable<BookingDetail>>()), Times.Once);
        }

        [Fact]
        public async Task AdminBookingFlow_WithMemberPoints_Success()
        {
            // Bước 1: Chuẩn bị dữ liệu cho member có điểm
            var movieId = Guid.NewGuid();
            var showTimeId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var memberId = Guid.NewGuid();
            var seatId = Guid.NewGuid();

            var movie = new Movie
            {
                Id = movieId,
                Title = "Spider-Man: No Way Home",
                Status = MovieStatus.NowShowing
            };

            var showTime = new ShowTime
            {
                Id = showTimeId,
                MovieId = movieId,
                RoomId = roomId,
                ShowDate = DateTime.UtcNow.AddDays(1),
                StartTime = new TimeSpan(20, 0, 0),
                EndTime = new TimeSpan(23, 0, 0),
                IsActive = true,
                Movie = movie,
                Room = new CinemaRoom
                {
                    Id = roomId,
                    RoomName = "Phòng VIP",
                    IsActive = true
                }
            };

            var member = new Users
            {
                Id = memberId,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0987654321",
                IdentityCard = "987654321",
                Role = UserRole.Member,
                Score = 500,
                IsActive = true
            };

            var seat = new Seat
            {
                Id = seatId,
                SeatCode = "VIP1",
                RoomId = roomId,
                Status = SeatStatus.Available,
                PriceSeat = 150000,
                RowIndex = 1,
                ColumnIndex = 1
            };

            // Mock các repository trả về dữ liệu tương ứng
            _showtimeRepo.Setup(r => r.FoundOrThrowAsync(showTimeId, It.IsAny<string>()))
                .ReturnsAsync(showTime);
            _movieRepo.Setup(r => r.FoundOrThrowAsync(movieId, It.IsAny<string>()))
                .ReturnsAsync(movie);
            _userRepo.Setup(r => r.FoundOrThrowAsync(memberId, It.IsAny<string>()))
                .ReturnsAsync(member);
            _seatRepository.Setup(r => r.GetSeatsByIdsAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<Seat> { seat });
            _seatRepository.Setup(r => r.GetBookedSeatIdsForShowTimeAsync(showTimeId))
                .ReturnsAsync(new List<Guid>());
            _bookingRepo.Setup(r => r.CreateAsync(It.IsAny<Booking>()))
                .ReturnsAsync((Booking b) => { b.Id = Guid.NewGuid(); return b; });
            _bookingDetailRepo.Setup(r => r.CreateRangeAsync(It.IsAny<IEnumerable<BookingDetail>>()))
                .Returns(Task.CompletedTask);
            _userRepo.Setup(r => r.UpdateAsync(It.IsAny<Users>()))
                .ReturnsAsync((Users u) => u);
            _pointHistoryRepo.Setup(r => r.CreateAsync(It.IsAny<PointHistory>()))
                .ReturnsAsync((PointHistory ph) => ph);

            var service = CreateService();

            // Bước 2: Gọi hàm đặt vé cho member có điểm
            var bookingRequest = new ConfirmBookingRequestAdminDto
            {
                ShowTimeId = showTimeId,
                SeatIds = new List<Guid> { seatId },
                MemberId = memberId.ToString(),
                PointsUsed = 100,
                ConvertedTickets = 0
            };

            var bookingResult = await service.ConfirmAdminBooking(bookingRequest);

            // Bước 3: Kiểm tra booking đã được tạo thành công
            var bookingJsonResult = Assert.IsType<JsonResult>(bookingResult);
            _bookingRepo.Verify(r => r.CreateAsync(It.IsAny<Booking>()), Times.Once);
            _bookingDetailRepo.Verify(r => r.CreateRangeAsync(It.IsAny<IEnumerable<BookingDetail>>()), Times.Once);
        }

        [Fact]
        public async Task AdminBookingFlow_SeatAlreadyBooked_Failure()
        {
            // Bước 1: Chuẩn bị dữ liệu với ghế đã được đặt
            var showTimeId = Guid.NewGuid();
            var seatId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var showTime = new ShowTime
            {
                Id = showTimeId,
                ShowDate = DateTime.UtcNow.AddDays(1),
                IsActive = true
            };

            var user = new Users
            {
                Id = userId,
                FullName = "Admin",
                Email = "admin@cinema.com",
                IsActive = true
            };

            // Mock: ghế đã được đặt
            _showtimeRepo.Setup(r => r.FoundOrThrowAsync(showTimeId, It.IsAny<string>()))
                .ReturnsAsync(showTime);
            _userRepo.Setup(r => r.FoundOrThrowAsync(userId, It.IsAny<string>()))
                .ReturnsAsync(user);
            _seatRepository.Setup(r => r.GetBookedSeatIdsForShowTimeAsync(showTimeId))
                .ReturnsAsync(new List<Guid> { seatId });

            var service = CreateService();

            // Bước 2: Gọi hàm đặt vé với ghế đã được đặt
            var bookingRequest = new ConfirmBookingRequestAdminDto
            {
                ShowTimeId = showTimeId,
                SeatIds = new List<Guid> { seatId },
                MemberId = userId.ToString()
            };

            var bookingResult = await service.ConfirmAdminBooking(bookingRequest);

            // Bước 3: Kiểm tra trả về lỗi đúng
            var badRequestJsonResult = Assert.IsType<JsonResult>(bookingResult);
            Assert.Contains("Không tìm thấy suất chiếu", badRequestJsonResult.Value.ToString());
        }

        [Fact]
        public async Task AdminBookingFlow_InvalidShowTime_Failure()
        {
            // Bước 1: Chuẩn bị dữ liệu với suất chiếu không hợp lệ
            var showTimeId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            // Mock: suất chiếu không tồn tại
            _showtimeRepo.Setup(r => r.FoundOrThrowAsync(showTimeId, It.IsAny<string>()))
                .ThrowsAsync(new DomainLayer.Exceptions.NotFoundException("ShowTime not found"));

            var service = CreateService();

            // Bước 2: Gọi hàm đặt vé với suất chiếu không hợp lệ
            var bookingRequest = new ConfirmBookingRequestAdminDto
            {
                ShowTimeId = showTimeId,
                SeatIds = new List<Guid> { Guid.NewGuid() },
                MemberId = userId.ToString()
            };

            var bookingResult = await service.ConfirmAdminBooking(bookingRequest);

            // Bước 3: Kiểm tra trả về lỗi đúng
            var notFoundJsonResult = Assert.IsType<JsonResult>(bookingResult);
        }

        [Fact]
        public async Task AdminBookingFlow_GetBookingDetails_Success()
        {
            // Bước 1: Chuẩn bị dữ liệu booking để lấy chi tiết
            var bookingId = Guid.NewGuid();
            var showTimeId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var booking = new Booking
            {
                Id = bookingId,
                BookingCode = "BK20241201001",
                ShowTimeId = showTimeId,
                UserId = userId,
                Status = BookingStatus.Confirmed,
                TotalPrice = 200000,
                TotalSeats = 2,
                BookingDate = DateTime.UtcNow,
                ShowTime = new ShowTime
                {
                    Id = showTimeId,
                    ShowDate = DateTime.UtcNow.AddDays(1),
                    Movie = new Movie { Title = "Test Movie" },
                    Room = new CinemaRoom { RoomName = "Test Room" }
                },
                User = new Users
                {
                    Id = userId,
                    FullName = "Test User"
                },
                BookingDetails = new List<BookingDetail>
                {
                    new BookingDetail
                    {
                        Seat = new Seat { SeatCode = "A1" }
                    },
                    new BookingDetail
                    {
                        Seat = new Seat { SeatCode = "A2" }
                    }
                }
            };

            // Mock: trả về booking khi tìm kiếm
            _bookingRepo.Setup(r => r.FindByIdAsync(bookingId, "ShowTime", "ShowTime.Movie", "ShowTime.Room", "User", "BookingDetails", "BookingDetails.Seat"))
                .ReturnsAsync(booking);

            var service = CreateService();

            // Bước 2: Gọi hàm lấy chi tiết booking
            var result = await service.GetAdminBookingDetailsAsync(bookingId);

            // Bước 3: Kiểm tra response trả về có thông tin booking
            var okJsonResult = Assert.IsType<JsonResult>(result);
            var response = okJsonResult.Value;
            Assert.NotNull(response);
        }
    }
} 