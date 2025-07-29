using Application.ResponseCode;
using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.DTO.UserManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using DomainLayer.Exceptions;
using InfrastructureLayer.Repository;
using InfrastructureLayer.Data;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using InfrastructureLayer.Core.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DomainLayer.Entities;
using DomainLayer.Enum;
using DomainLayer.Entities;
using DomainLayer.Enum;

namespace ApplicationLayer.Services.BookingTicketManagement
{
    public interface IBookingTicketService
    {
        Task<IActionResult> GetAvailableMovies(); //Lấy danh sách phim
        Task<IActionResult> GetShowDatesByMovie(Guid movieId); //Lấy danh sách ngày chiếu cho phim
        Task<IActionResult> GetShowTimesByMovieAndDate(Guid movieId, DateTime selectedDate); //Lấy các giờ chiếu trong ngày
        Task<IActionResult> ConfirmUserBooking(ConfirmBookingRequestDto request);
        Task<IActionResult> ConfirmUserBookingV2(ConfirmBookingRequestDto request);

        Task<IActionResult> CheckMember(CheckMemberRequestDto request);

        Task<(bool Success, String message)> CreateMemberAccount(CreateMemberAccountDto request);
        Task<IActionResult> ConfirmAdminBooking(ConfirmBookingRequestAdminDto request);
        Task<IActionResult> GetBookingDetails(string bookingCode);
        
        Task<IActionResult> GetBookingDetailsAsync(Guid bookingId, Guid userId);

        // New methods for score conversion booking
        Task<IActionResult> GetBookingConfirmationDetailAsync(Guid showTimeId, List<Guid> seatIds, string memberId);
        Task<IActionResult> ConfirmBookingWithScoreAsync(BookingConfirmWithScoreRequestDto request);

        // Booking list management methods
        Task<IActionResult> GetBookingListAsync(BookingFilterDto filter);
        Task<IActionResult> UpdateBookingStatusAsync(Guid bookingId, string newStatus);
        Task<IActionResult> CancelBookingAsync(Guid bookingId, string reason);

        // Booking history for member dashboard
        Task<IActionResult> GetUserBookingHistoryAsync(Guid userId);

        Task<IActionResult> GetAdminBookingDetailsAsync(Guid bookingId);
    }

    public class BookingTicketService : IBookingTicketService
    {
        private readonly IGenericRepository<Movie> _movieRepo;
        private readonly IGenericRepository<ShowTime> _showtimeRepo;
        private readonly IGenericRepository<Booking> _bookingRepo;
        private readonly IGenericRepository<BookingDetail> _bookingDetailRepo;
        private readonly IGenericRepository<Users> _userRepo;
        private readonly IGenericRepository<PointHistory> _pointHistoryRepo;
        private readonly ISeatRepository _seatRepository;
        private readonly IGenericRepository<Promotion> _promotionRepo;
        private readonly IGenericRepository<UserPromotion> _userPromotionRepo;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IMailService _mailService;
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingTicketService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MovieContext _context;

        public BookingTicketService(
            IUserRepository userRepository,
            IGenericRepository<Movie> movieRepo,
            IGenericRepository<ShowTime> showtimeRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<BookingDetail> bookingDetailRepo,
            IGenericRepository<Users> userRepo,
            IGenericRepository<PointHistory> pointHistoryRepo,
            ISeatRepository seatRepository,
            IGenericRepository<Promotion> promotionRepo,
            IGenericRepository<UserPromotion> userPromotionRepo,
            IMapper mapper,
            IMailService mailService,
            IBookingRepository bookingRepository,
            ILogger<BookingTicketService> logger,
            IHttpContextAccessor httpContextAccessor,
            MovieContext context)
        {
            _userRepository = userRepository;
            _movieRepo = movieRepo;
            _showtimeRepo = showtimeRepo;
            _bookingRepo = bookingRepo;
            _bookingDetailRepo = bookingDetailRepo;
            _userRepo = userRepo;
            _pointHistoryRepo = pointHistoryRepo;
            _seatRepository = seatRepository;
            _promotionRepo = promotionRepo;
            _userPromotionRepo = userPromotionRepo;
            _mapper = mapper;
            _mailService = mailService;
            _bookingRepository = bookingRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }



        public async Task<IActionResult> GetAvailableMovies()
        {
            var movie = await _movieRepo.WhereAsync(m => m.Status == MovieStatus.NowShowing, "MovieImages", "MovieGenres.Genre");
            if (movie == null)
                return ErrorResp.NotFound("Movie Not Found");

            var result = _mapper.Map<List<MovieDropdownDto>>(movie);

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetShowDatesByMovie(Guid movieId)
        {
            // Lấy tất cả lịch chiếu còn hoạt động của phim
            var showtimes = await _showtimeRepo.WhereAsync(s => s.MovieId == movieId && s.IsActive);
            if (showtimes == null || !showtimes.Any())
            {
                return ErrorResp.NotFound("No show dates found for this movie.");
            }

            var today = DateTime.Now.Date;
            // Filter showtimes có ngày chiếu từ hôm nay trở đi
            var dates = showtimes
                .Where(s => s.ShowDate.HasValue && s.ShowDate!.Value.ToLocalTime().Date >= today)
                .Select(s => s.ShowDate!.Value.ToLocalTime().Date)
                .Distinct()
                .OrderBy(d => d)
                .Select(d => new
                {
                    Code = d.ToString("yyyy-MM-dd"),
                    Text = d.ToString("dd/MM")
                })
                .ToList();

            return SuccessResp.Ok(dates);
        }

        public async Task<IActionResult> GetShowTimesByMovieAndDate(Guid movieId, DateTime selectedDate)
        {
            // Chỉ lấy lịch chiếu còn hoạt động
            var showtimes = await _showtimeRepo.WhereAsync(s =>
                s.MovieId == movieId &&
                s.ShowDate.HasValue &&
                s.IsActive);

            var today = DateTime.Now.Date;
            var nowTime = DateTime.Now.TimeOfDay;
            // Convert ShowDate về LocalTime để so sánh chính xác theo ngày địa phương
            var filteredShowtimes = showtimes
                .Where(s =>
                    {
                        var localDate = s.ShowDate!.Value.ToLocalTime().Date;
                        if (localDate < today) return false;
                        if (localDate == today && s.StartTime <= nowTime) return false; // bỏ giờ đã qua hôm nay
                        return true;
                    });

            var timeList = filteredShowtimes
                .Where(s => s.ShowDate!.Value.ToLocalTime().Date == selectedDate.Date)
                .GroupBy(s => s.StartTime)
                .Select(g => new
                {
                    Id = g.First().Id,
                    Time = g.Key.ToString(@"hh\:mm"),
                    FullDate = g.First().ShowDate!.Value
                })
                .OrderBy(t => t.Time)
                .ToList();

            return SuccessResp.Ok(timeList);
        }

        public async Task<IActionResult> ConfirmUserBooking(ConfirmBookingRequestDto request)
        {
            try
            {
                // AC-02: Tất cả các chi tiết đặt vé phải dựa trên các lựa chọn trước đó và dữ liệu tài khoản của người dùng.
                // 1. Lấy thông tin ShowTime và Room
                var showTime = await _showtimeRepo.FoundOrThrowAsync(request.ShowtimeId, "ShowTime not found.");
                var movie = await _movieRepo.FoundOrThrowAsync(showTime.MovieId, "Movie not found.");
                // Bạn có thể lấy thông tin User từ UserId trong request, hoặc từ context nếu đã xác thực

                // Kiểm tra lại các ghế đã chọn có còn trống không
                var bookedSeatIdsForShowTime = await _seatRepository.GetBookedSeatIdsForShowTimeAsync(request.ShowtimeId);
                foreach (var seatId in request.SeatIds)
                {
                    if (bookedSeatIdsForShowTime.Contains(seatId))
                    {
                        // AC-05: Nếu quá trình gửi thất bại...
                        return ErrorResp.BadRequest($"Seat with ID {seatId} is already booked.");
                    }

                    var seat = await _seatRepository.GetByIdAsync(seatId); // GetByIdAsync của bạn kiểm tra IsActive
                    if (seat == null)
                    {
                        return ErrorResp.BadRequest($"Seat with ID {seatId} is no longer available or does not exist.");
                    }
                }

                // 3. Tạo Booking mới
                var newBooking = new Booking
                {
                    BookingCode = GenerateBookingCode(), // Hàm tạo mã booking duy nhất
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = request.TotalPrice,
                    Status = BookingStatus.Confirmed, // Trạng thái ban đầu là Confirmed
                    TotalSeats = request.SeatIds.Count,
                    UserId = request.UserId,
                    ShowTimeId = request.ShowtimeId
                };

                await _bookingRepo.CreateAsync(newBooking);

                // 4. Tạo BookingDetails cho từng ghế
                var bookingDetails = new List<BookingDetail>();
                foreach (var seatId in request.SeatIds)
                {
                    var seat = await _seatRepository.GetByIdAsync(seatId); // Lấy lại thông tin ghế để lấy giá
                    if (seat == null)
                    {
                        // Should not happen if validation passed, but good for defensive programming
                        throw new NotFoundException($"Seat with ID {seatId} not found during booking creation.");
                    }

                    bookingDetails.Add(new BookingDetail
                    {
                        BookingId = newBooking.Id,
                        SeatId = seatId,
                        Price = seat.PriceSeat // Lấy giá từ ghế
                    });

                    // Cập nhật trạng thái ghế (set IsActive = false)
                    seat.Status = SeatStatus.Selected;
                    await _seatRepository.UpdateSeatAsync(seat);
                }
                await _bookingDetailRepo.CreateRangeAsync(bookingDetails);

                // AC-04: Người dùng phải được chuyển hướng đến màn hình "Thông tin đặt vé / Thành công"
                // Trả về thông tin cần thiết cho màn hình thành công
                var successResponse = new BookingConfirmationSuccessDto // Cần tạo DTO này
                {
                    BookingCode = newBooking.BookingCode,
                    MovieTitle = movie.Title, // Lấy từ movie entity
                    CinemaRoomName = showTime.Room.RoomName, // Cần load Room nếu chưa include
                    ShowDate = showTime.ShowDate.HasValue ? showTime.ShowDate.Value.ToString("dd/MM/yyyy") : "",
                    ShowTime = showTime.ShowDate.HasValue ? showTime.ShowDate.Value.ToString("HH:mm") : "",
                    BookedSeats = bookingDetails.Select(bd => new SeatBookingDto { SeatId = bd.SeatId, SeatCode = _seatRepository.GetByIdAsync(bd.SeatId).Result?.SeatCode ?? "" }).ToList(), // Cần lấy SeatCode
                    TotalPrice = newBooking.TotalPrice,
                    FullName = request.FullName,
                    Email = request.Email,
                    IdentityCard = request.IdentityCard,
                    PhoneNumber = request.PhoneNumber
                };

                // 5. Cộng điểm và tự động cấp voucher nếu đủ điểm
                await AwardPointsAndVouchersAsync(request.UserId, request.SeatIds.Count, newBooking.Id, newBooking.BookingCode);

                // 6. Gửi email xác nhận cho người dùng (nếu cần)
                return SuccessResp.Ok(successResponse);
            }
            catch (NotFoundException ex)
            {
                // AC-05: Nếu quá trình gửi thất bại...
                return ErrorResp.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình xử lý");
                // AC-05: Nếu quá trình gửi thất bại. Vui lòng thử lại sau.
                return ErrorResp.InternalServerError("Đặt vé thất bại. Vui lòng thử lại sau.");
            }
        }

        public async Task<IActionResult> ConfirmUserBookingV2(ConfirmBookingRequestDto request)
        {
            try
            {
                // Bước 1: Validate thông tin đầu vào
                if (request.SeatIds == null || !request.SeatIds.Any())
                {
                    return ErrorResp.BadRequest("Vui lòng chọn ít nhất một ghế.");
                }

                if (request.TotalPrice <= 0)
                {
                    return ErrorResp.BadRequest("Tổng tiền phải lớn hơn 0.");
                }

                // Bước 2: Kiểm tra thông tin ShowTime
                var showTime = await _showtimeRepo.FirstOrDefaultAsync(
                    st => st.Id == request.ShowtimeId,
                    "Movie", "Room"
                );

                if (showTime == null)
                {
                    return ErrorResp.NotFound("Không tìm thấy suất chiếu.");
                }

                // Kiểm tra suất chiếu có hợp lệ (chưa diễn ra)
                if (showTime.ShowDate < DateTime.UtcNow)
                {
                    return ErrorResp.BadRequest("Suất chiếu đã qua. Không thể đặt vé.");
                }

                // Bước 3: Kiểm tra thông tin User
                var user = await _userRepo.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return ErrorResp.NotFound("Không tìm thấy thông tin người dùng.");
                }

                // Bước 4: Kiểm tra tính hợp lệ của ghế
                var selectedSeats = await _seatRepository.GetSeatsByIdsAsync(request.SeatIds);
                var bookedSeatIds = await _seatRepository.GetBookedSeatIdsForShowTimeAsync(request.ShowtimeId);

                foreach (var seatId in request.SeatIds)
                {
                    // Kiểm tra ghế có tồn tại
                    var seat = selectedSeats.FirstOrDefault(s => s.Id == seatId);
                    if (seat == null)
                    {
                        return ErrorResp.BadRequest($"Ghế với ID {seatId} không tồn tại.");
                    }

                    // Kiểm tra ghế có thuộc phòng chiếu và còn hoạt động
                    if (seat.RoomId != showTime.RoomId || seat.Status != SeatStatus.Available)
                    {
                        return ErrorResp.BadRequest($"Ghế {seat.SeatCode} không khả dụng hoặc không thuộc phòng chiếu này.");
                    }

                    // Kiểm tra ghế đã được đặt chưa
                    if (bookedSeatIds.Contains(seatId))
                    {
                        return ErrorResp.BadRequest($"Ghế {seat.SeatCode} đã được đặt.");
                    }
                }

                // Bước 5: Tính toán lại tổng tiền để đảm bảo tính chính xác
                var calculatedTotalPrice = selectedSeats.Sum(s => s.PriceSeat);
                if (Math.Abs(calculatedTotalPrice - request.TotalPrice) > 0.01m)
                {
                    _logger.LogWarning("Tổng tiền không khớp. Calculated: {Calculated}, Request: {Request}", 
                        calculatedTotalPrice, request.TotalPrice);
                    // Có thể chọn cách xử lý: dùng giá tính toán hoặc báo lỗi
                    // return ErrorResp.BadRequest($"Tổng tiền không chính xác. Giá thực tế: {calculatedTotalPrice:C}");
                }

                // Bước 6: Tạo Booking trong transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Tạo booking record
                    var booking = new Booking
                    {
                        BookingCode = GenerateBookingCode(),
                        BookingDate = DateTime.UtcNow,
                        TotalPrice = calculatedTotalPrice,
                        Status = BookingStatus.Pending,
                        TotalSeats = request.SeatIds.Count,
                        UserId = request.UserId,
                        ShowTimeId = request.ShowtimeId,
                        FullName = request.FullName,
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        IdentityCardNumber = request.IdentityCard
                    };

                    await _bookingRepo.CreateAsync(booking);

                    // Tạo booking details và cập nhật trạng thái ghế
                    var bookingDetails = new List<BookingDetail>();
                    foreach (var seat in selectedSeats)
                    {
                        bookingDetails.Add(new BookingDetail
                        {
                            BookingId = booking.Id,
                            SeatId = seat.Id,
                            Price = seat.PriceSeat
                        });

                        // Đánh dấu ghế không khả dụng
                        seat.Status = SeatStatus.Selected;
                    }

                    // Cập nhật tất cả ghế cùng lúc
                    await _seatRepository.UpdateSeatsAsync(selectedSeats);

                    await _bookingDetailRepo.CreateRangeAsync(bookingDetails);

                    // Commit transaction
                    await transaction.CommitAsync();

                    // Bước 7: Chuẩn bị response
                    var response = new BookingConfirmationSuccessDto
                    {
                        BookingCode = booking.BookingCode,
                        MovieTitle = showTime.Movie.Title,
                        CinemaRoomName = showTime.Room.RoomName,
                        ShowDate = showTime.ShowDate?.ToString("dd/MM/yyyy") ?? "",
                        ShowTime = showTime.ShowDate?.ToString("HH:mm") ?? "",
                        BookedSeats = selectedSeats.Select(s => new SeatBookingDto
                        {
                            SeatId = s.Id,
                            SeatCode = s.SeatCode
                        }).ToList(),
                        TotalPrice = calculatedTotalPrice,
                        FullName = request.FullName,
                        Email = request.Email,
                        IdentityCard = request.IdentityCard,
                        PhoneNumber = request.PhoneNumber
                    };

                    _logger.LogInformation("Đặt vé thành công. BookingCode: {BookingCode}", booking.BookingCode);
                    return SuccessResp.Ok(response);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý đặt vé: {Message}", ex.Message);
                return ErrorResp.InternalServerError("Đặt vé thất bại. Vui lòng thử lại sau.");
            }
        }

        private string GenerateBookingCode()
        {
            // Logic tạo mã booking duy nhất (ví dụ: kết hợp thời gian và random string)
            return "BK" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();

        }

        public async Task<IActionResult> GetBookingDetailsAsync(Guid bookingId, Guid userId)
        {
            // Get booking with details
            var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId, userId);
            if (booking == null)
            {
                return ErrorResp.NotFound("Booking not found");
            }

            // Check if booking has all required data
            if (booking.ShowTime?.Movie == null || booking.ShowTime?.Room == null ||
                !booking.BookingDetails.Any() || booking.User == null)
            {
                return ErrorResp.BadRequest("Incomplete booking data. Please return and complete seat selection.");
            }

            // Map booking to DTO
            var bookingDetailsDto = new BookingDetailsDto
            {
                BookingId = booking.Id,
                BookingCode = booking.BookingCode,
                MovieName = booking.ShowTime.Movie.Title,
                RoomName = booking.ShowTime.Room.RoomName,
                ShowDate = booking.ShowTime.ShowDate ?? DateTime.UtcNow,
                SeatCodes = booking.BookingDetails.Select(bd => bd.Seat.SeatCode).ToList(),
                UnitPrice = booking.BookingDetails.FirstOrDefault()?.Price ?? 0,
                TotalPrice = booking.TotalPrice,
                TotalSeats = booking.TotalSeats,
                FullName = booking.User.FullName,
                Email = booking.User.Email,
                IdentityCard = booking.User.IdentityCard,
                Phone = booking.User.Phone
            };

            return SuccessResp.Ok(bookingDetailsDto);
        }

        public async Task<IActionResult> CheckMember(CheckMemberRequestDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.MemberId) && string.IsNullOrEmpty(request.IdentityNumber))
                {
                    return ErrorResp.BadRequest("Vui lòng cung cấp ID thành viên hoặc số CMND");
                }

                Users? member = null;

                // Tìm bằng MemberId (Guid)
                if (!string.IsNullOrEmpty(request.MemberId))
                {
                    if (Guid.TryParse(request.MemberId, out var memberId))
                    {
                        member = await _userRepo.FirstOrDefaultAsync(u =>
                            u.Id == memberId &&
                            u.Role == UserRole.Member);
                    }
                    else
                    {
                        return ErrorResp.BadRequest("ID thành viên không hợp lệ");
                    }
                }

                // Tìm bằng CMND nếu chưa tìm thấy
                if (member == null && !string.IsNullOrEmpty(request.IdentityNumber))
                {
                    member = await _userRepo.FirstOrDefaultAsync(u =>
                        u.IdentityCard == request.IdentityNumber &&
                        u.Role == UserRole.Member);
                }

                if (member == null)
                {
                    return ErrorResp.NotFound("Không tìm thấy thành viên nào!"); 
                }  

                // Sử dụng AutoMapper
                var memberInfo = _mapper.Map<MemberInfoDto>(member);
                return SuccessResp.Ok(memberInfo);
            }
            catch (Exception ex)
            {
                return ErrorResp.InternalServerError("Lỗi hệ thống khi kiểm tra thành viên");
            }
        }

        public async Task<(bool Success, String message)> CreateMemberAccount(CreateMemberAccountDto request)
        {
            try
            {
                if (await _userRepository.IsPhoneExistsAsync(request.PhoneNumber))
                {
                    return (false, "Số điện thoại đã tồn tại");
                }

                if (await _userRepository.IsEmailExistsAsync(request.Email))
                {
                    return (false, "Email đã tồn tại");
                }

                if (await _userRepository.IsIdentityCardExistsAsync(request.IdentityCard))
                {
                    return (false, "CMND/CCCD đã tồn tại");
                }
                //var password = GenerateRandomPassword(8, true, true, true, true);
                var password = "123456";
                // Tạo người dùng mới
                var  newUsers = new Users
                {
                    Username = request.Email,
                    Password = HashPassword(password),
                    Email = request.Email,
                    FullName = request.FullName,
                    Phone = request.PhoneNumber,
                    IdentityCard = request.IdentityCard,
                    Address = "",
                    BirthDate = DateTime.UtcNow,
                    Gender = UserGender.Male
                };

                await _userRepository.CreateAsync(newUsers);

                string subject = "Thông tin đăng nhập hệ thống Cinema City";
                string htmlMessage = $@"
                    <h3>Chào {request.Email},</h3>
                    <p>Bạn đã đăng ký tài khoản thành công.</p>
                    <p><strong>Tên đăng nhập:</strong> {request.Email}</p>
                    <p><strong>Mật khẩu:</strong> {password}</p>
                    <p>Vui lòng đăng nhập và thay đổi mật khẩu sau lần đăng nhập đầu tiên.</p>
                    <br/>
                    <p>Trân trọng,<br/>Cinema City</p>
                ";

                // 4. Gửi email
                await _mailService.SendEmailAsync(request.Email, subject, htmlMessage);

                return (true, "Resgiter Successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<IActionResult> ConfirmAdminBooking(ConfirmBookingRequestAdminDto request)
        {
            try
            {
                // Validate seats
                var seatValidation = await ValidateSelectedSeats(request.ShowTimeId, request.SeatIds);
                if (seatValidation is not OkObjectResult okResult || !(okResult.Value is bool isValid) || !isValid)
                {
                    return seatValidation;
                }

                // Get showtime and movie info
                var showTime = await _showtimeRepo.FoundOrThrowAsync(request.ShowTimeId, "ShowTime not found");
                var movie = await _movieRepo.FoundOrThrowAsync(showTime.MovieId, "Movie not found");

                // Calculate total price
                var seats = await _seatRepository.GetSeatsByIdsAsync(request.SeatIds);
                var totalPrice = seats.Sum(s => s.PriceSeat);

                // Handle member points if applicable
                double pointsUsed = 0;
                double remainingPoints = 0;
                Users? member = null;

                if (!string.IsNullOrEmpty(request.MemberId))
                {
                    member = await _userRepo.FoundOrThrowAsync(Guid.Parse(request.MemberId), "Member not found");

                    if (request.PointsUsed.HasValue && request.PointsUsed > 0)
                    {
                        if (member.Score < request.PointsUsed.Value)
                        {
                            return ErrorResp.BadRequest("Điểm thành viên không đủ để đổi thành vé");
                        }

                        pointsUsed = request.PointsUsed.Value;
                        remainingPoints = member.Score - pointsUsed;

                        // Apply discount (example: 100 points = 10,000 VND discount)
                        var discount = (decimal)(pointsUsed / 100) * 10000;
                        totalPrice = Math.Max(0, totalPrice - discount);
                    }
                }

                // Create booking
                var booking = new Booking
                {
                    BookingCode = GenerateBookingCode(),
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = totalPrice,
                    Status = BookingStatus.Confirmed,
                    TotalSeats = request.SeatIds.Count,
                    UserId = member?.Id ?? Guid.Empty,
                    ShowTimeId = request.ShowTimeId,
                    ConvertedTickets = request.ConvertedTickets,
                    PointsUsed = pointsUsed
                };

                await _bookingRepo.CreateAsync(booking);

                // Create booking details
                var bookingDetails = seats.Select(seat => new BookingDetail
                {
                    BookingId = booking.Id,
                    SeatId = seat.Id,
                    Price = seat.PriceSeat
                }).ToList();

                await _bookingDetailRepo.CreateRangeAsync(bookingDetails);

                // Update member points if applicable
                if (member != null && pointsUsed > 0)
                {
                    member.Score = remainingPoints;
                    await _userRepo.UpdateAsync(member);

                    // Record point history
                    var pointHistory = new PointHistory
                    {
                        Points = pointsUsed,
                        Type = PointType.Used,
                        Description = $"Used for booking {booking.BookingCode}",
                        IsUsed = true,
                        UserId = member.Id,
                        BookingId = booking.Id
                    };
                    await _pointHistoryRepo.CreateAsync(pointHistory);
                }

                // Prepare response
                var response = new BookingConfirmationDto
                {
                    BookingCode = booking.BookingCode,
                    MovieTitle = movie.Title,
                    CinemaRoom = showTime.Room?.RoomName ?? "Unknown",
                    ShowDate = showTime.ShowDate?.ToString("dd/MM/yyyy") ?? "",
                    ShowTime = showTime.ShowDate?.ToString("HH:mm") ?? "",
                    Seats = seats.Select(s => new SeatSelectionDto
                    {
                        Id = s.Id,
                        SeatCode = s.SeatCode,
                        Price = s.PriceSeat
                    }).ToList(),
                    SubTotal = seats.Sum(s => s.PriceSeat),
                    Discount = seats.Sum(s => s.PriceSeat) - totalPrice,
                    Total = totalPrice,
                    PointsUsed = pointsUsed,
                    RemainingPoints = remainingPoints,
                    PaymentMethod = request.PaymentMethod,
                    BookingDate = booking.BookingDate.ToString("dd/MM/yyyy HH:mm:ss")
                };

                return SuccessResp.Ok(response);
            }
            catch (Exception ex)
            {
                return ErrorResp.InternalServerError(ex.Message);
            }
        }


        public async Task<IActionResult> GetBookingDetails(string bookingCode)
        {
            try
            {
                var booking = await _bookingRepo.FirstOrDefaultAsync(
                    b => b.BookingCode == bookingCode,
                    "User,ShowTime,ShowTime.Movie,ShowTime.Room,BookingDetails,BookingDetails.Seat");

                if (booking == null)
                {
                    return ErrorResp.NotFound("Booking not found");
                }

                var response = new
                {
                    bookingCode = booking.BookingCode,
                    status = booking.Status.ToString(),
                    movieTitle = booking.ShowTime?.Movie?.Title,
                    cinemaRoom = booking.ShowTime?.Room?.RoomName,
                    showDate = booking.ShowTime?.ShowDate?.ToString("dd/MM/yyyy"),
                    showTime = booking.ShowTime?.ShowDate?.ToString("HH:mm"),
                    seats = booking.BookingDetails?.Select(bd => bd.Seat?.SeatCode).ToList(),
                    totalPrice = booking.TotalPrice,
                    memberInfo = booking.User == null ? null : new
                    {
                        memberId = booking.User.Id,
                        fullName = booking.User.FullName
                    },
                    bookingDate = booking.BookingDate.ToString("dd/MM/yyyy HH:mm:ss"),

                };

                return SuccessResp.Ok(response);
            }
            catch (Exception ex)
            {
                return ErrorResp.InternalServerError(ex.Message);
            }
        }

        private async Task<IActionResult> ValidateSelectedSeats(Guid showTimeId, List<Guid> seatIds)
        {
            if (seatIds == null || seatIds.Count == 0)
                return ErrorResp.BadRequest("Vui lòng chọn ít nhất 1 ghế");

            if (seatIds.Count > 8)
                return ErrorResp.BadRequest("Tối đa 8 ghế mỗi lần đặt");

            var showTime = await _showtimeRepo.FindByIdAsync(showTimeId, "Room");
            if (showTime == null)
                return ErrorResp.NotFound("Không tìm thấy suất chiếu");

            var seats = await _seatRepository.GetSeatsByIdsAsync(seatIds);
            if (seats.Count != seatIds.Count)
                return ErrorResp.BadRequest("Một số ghế đã chọn không tồn tại");

            var bookedSeatIds = await _seatRepository.GetBookedSeatIdsForShowTimeAsync(showTimeId);
            var alreadyBooked = seats.Where(s => bookedSeatIds.Contains(s.Id)).ToList();

            if (alreadyBooked.Any())
            {
                var bookedCodes = alreadyBooked.Select(s => s.SeatCode);
                return ErrorResp.BadRequest($"Ghế {string.Join(", ", bookedCodes)} đã được đặt");
            }

            return SuccessResp.Ok(true);
        }
    

    public static string GenerateRandomPassword(int length = 8, bool includeUppercase = true, bool includeLowercase = true, bool includeNumbers = true, bool includeSpecialChars = true)
        {
            const string UPPER = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string LOWER = "abcdefghijklmnopqrstuvwxyz";
            const string DIGITS = "0123456789";
            const string SPECIAL = "!@#$%^&*()-_=+[]{};:,.<>?/";

            StringBuilder characterSet = new StringBuilder();
            if (includeUppercase) characterSet.Append(UPPER);
            if (includeLowercase) characterSet.Append(LOWER);
            if (includeNumbers) characterSet.Append(DIGITS);
            if (includeSpecialChars) characterSet.Append(SPECIAL);

            if (characterSet.Length == 0)
                throw new ArgumentException("Phải chọn ít nhất một loại ký tự để tạo mật khẩu.");

            StringBuilder password = new StringBuilder();
            Random rand = new Random();

            for (int i = 0; i < length; i++)
            {
                int index = rand.Next(characterSet.Length);
                password.Append(characterSet[index]);
            }

            return password.ToString();
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<IActionResult> GetBookingConfirmationDetailAsync(Guid showTimeId, List<Guid> seatIds, string memberId)
        {
            try
            {
                // 1. Validate showtime and movie
                var showTime = await _showtimeRepo.FoundOrThrowAsync(showTimeId, "ShowTime not found.");
                var movie = await _movieRepo.FoundOrThrowAsync(showTime.MovieId, "Movie not found.");

                // 2. Validate member
                var member = await _userRepo.FirstOrDefaultAsync(u => u.Id.ToString() == memberId);
                if (member == null)
                    return ErrorResp.NotFound("Member not found");

                // 3. Validate and calculate seat pricing
                var seatDetails = new List<SeatDetailForConfirmDto>();
                decimal totalPrice = 0;

                foreach (var seatId in seatIds)
                {
                    var seat = await _seatRepository.GetByIdAsync(seatId);
                    if (seat == null)
                        return ErrorResp.BadRequest($"Seat {seatId} not found");

                    seatDetails.Add(new SeatDetailForConfirmDto
                    {
                        Id = seat.Id,
                        SeatCode = seat.SeatCode,
                        Price = seat.PriceSeat
                    });

                    totalPrice += seat.PriceSeat;
                }

                // 4. Calculate score conversion capabilities (AC-02, AC-03)
                const double SCORE_PER_TICKET = 100.0; // Define your business rule
                int maxTicketsFromScore = (int)(member.Score / SCORE_PER_TICKET);
                bool canConvertScore = maxTicketsFromScore > 0 && seatDetails.Count > 0;

                // 5. Build response DTO (AC-01)
                var confirmationDetail = new BookingConfirmationDetailDto
                {
                    // Booking Information (Read-only fields)
                    BookingId = Guid.NewGuid().ToString(), // Temporary ID for display
                    MovieName = movie.Title,
                    Screen = showTime.Room?.RoomName ?? "Screen 1", // Get from relationship if available
                    Date = showTime.ShowDate?.ToString("dd/MM/yyyy") ?? "",
                    Time = showTime.ShowDate?.ToString("HH:mm") ?? "",
                    Seat = string.Join(", ", seatDetails.Select(s => s.SeatCode)),
                    Price = totalPrice,
                    Total = totalPrice,

                    // Member Information (Read-only fields)
                    MemberId = member.Id.ToString(),
                    FullName = member.FullName ?? "",
                    MemberScore = member.Score,
                    IdentityCard = member.IdentityCard ?? "",
                    PhoneNumber = member.Phone ?? "",

                    // Additional calculation info
                    SeatDetails = seatDetails,
                    CanConvertScore = canConvertScore,
                    MaxTicketsFromScore = Math.Min(maxTicketsFromScore, seatDetails.Count),
                    ScorePerTicket = SCORE_PER_TICKET
                };

                return SuccessResp.Ok(confirmationDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking confirmation detail");
                return ErrorResp.InternalServerError("Error retrieving booking details");
            }
        }

        public async Task<IActionResult> ConfirmBookingWithScoreAsync(BookingConfirmWithScoreRequestDto request)
        {
            try
            {
                // 1. Validate showtime and movie
                var showTime = await _showtimeRepo.FoundOrThrowAsync(request.ShowTimeId, "ShowTime not found.");
                var movie = await _movieRepo.FoundOrThrowAsync(showTime.MovieId, "Movie not found.");

                // 2. Validate member
                var member = await _userRepo.FirstOrDefaultAsync(u => u.Id.ToString() == request.MemberId);
                if (member == null)
                    return ErrorResp.NotFound("Member not found");

                // 3. Validate and calculate seat pricing
                var seatDetails = new List<SeatDetailForConfirmDto>();
                decimal totalPrice = 0;

                foreach (var seatId in request.SeatIds)
                {
                    var seat = await _seatRepository.GetByIdAsync(seatId);
                    if (seat == null)
                        return ErrorResp.BadRequest($"Seat {seatId} not found");

                    // Check if seat is still available
                    var bookedSeats = await _seatRepository.GetBookedSeatIdsForShowTimeAsync(request.ShowTimeId);
                    if (bookedSeats.Contains(seatId))
                        return ErrorResp.BadRequest($"Seat {seat.SeatCode} is already booked");

                    seatDetails.Add(new SeatDetailForConfirmDto
                    {
                        Id = seat.Id,
                        SeatCode = seat.SeatCode,
                        Price = seat.PriceSeat
                    });

                    totalPrice += seat.PriceSeat;
                }

                // 4. Handle score conversion logic (AC-02, AC-03, AC-04)
                const double SCORE_PER_TICKET = 100.0;
                decimal scoreDiscount = 0;
                double scoreToDeduct = 0;
                int actualTicketsConverted = 0;

                if (request.UseScoreConversion && request.TicketsToConvert > 0)
                {
                    // AC-03: Check if score is sufficient
                    double requiredScore = request.TicketsToConvert * SCORE_PER_TICKET;
                    if (member.Score < requiredScore)
                    {
                        return ErrorResp.BadRequest("Not enough score to convert into ticket");
                    }

                    // Validate tickets to convert doesn't exceed available seats
                    if (request.TicketsToConvert > request.SeatIds.Count)
                    {
                        return ErrorResp.BadRequest("Cannot convert more tickets than seats selected");
                    }

                    // AC-04: Calculate conversion if score is sufficient
                    actualTicketsConverted = request.TicketsToConvert;
                    scoreToDeduct = actualTicketsConverted * SCORE_PER_TICKET;
                    
                    // Calculate discount (assume 1 ticket = price of cheapest seat)
                    var cheapestSeatPrice = seatDetails.Min(s => s.Price);
                    scoreDiscount = actualTicketsConverted * cheapestSeatPrice;
                }

                // 5. Calculate final total
                decimal finalTotal = Math.Max(0, totalPrice - scoreDiscount);

                // --- Áp dụng khuyến mãi nếu có ---
                decimal promotionDiscount = 0;
                int? discountPercent = null;
                if (request.PromotionId.HasValue)
                {
                    var promotion = await _promotionRepo.FindByIdAsync(request.PromotionId.Value);
                    if (promotion != null && promotion.StartDate <= DateTime.UtcNow && promotion.EndDate >= DateTime.UtcNow)
                    {
                        discountPercent = promotion.DiscountPercent;
                        if (discountPercent > 0)
                        {
                            promotionDiscount = Math.Round(finalTotal * discountPercent.Value / 100);
                            finalTotal = Math.Max(0, finalTotal - promotionDiscount);
                        }
                    }
                }
                // --- END Áp dụng khuyến mãi ---

                // 6. Create booking (AC-05: Finalize booking regardless of score usage)
                var newBooking = new Booking
                {
                    BookingCode = GenerateBookingCode(),
                    BookingDate = DateTime.UtcNow,
                    TotalPrice = finalTotal,
                    Status = BookingStatus.Pending,
                    TotalSeats = request.SeatIds.Count,
                    UserId = Guid.Parse(request.MemberId),
                    ShowTimeId = request.ShowTimeId,
                    
                    // Required fields from member data
                    FullName = member.FullName ?? "Unknown",
                    Email = member.Email ?? "unknown@example.com",
                    PhoneNumber = member.Phone ?? "0000000000",
                    IdentityCardNumber = member.IdentityCard ?? "000000000000",
                    
                    // Score conversion fields
                    ConvertedTickets = actualTicketsConverted > 0 ? actualTicketsConverted : null,
                    PointsUsed = scoreToDeduct > 0 ? scoreToDeduct : null
                };

                await _bookingRepo.CreateAsync(newBooking);

                // 7. Create booking details for each seat
                var bookingDetails = new List<BookingDetail>();
                foreach (var seatDetail in seatDetails)
                {
                    var seat = await _seatRepository.GetByIdAsync(seatDetail.Id);
                    
                    bookingDetails.Add(new BookingDetail
                    {
                        BookingId = newBooking.Id,
                        SeatId = seatDetail.Id,
                        Price = seatDetail.Price
                    });

                    // Mark seat as unavailable
                    seat.Status = SeatStatus.Selected;
                    await _seatRepository.UpdateSeatAsync(seat);
                }
                await _bookingDetailRepo.CreateRangeAsync(bookingDetails);

                // 8. Update member score if conversion was used (AC-04)
                double remainingScore = member.Score;
                if (actualTicketsConverted > 0)
                {
                    member.Score -= scoreToDeduct;
                    await _userRepo.UpdateAsync(member);
                    remainingScore = member.Score;

                    // Create point history record
                    await _pointHistoryRepo.CreateAsync(new PointHistory
                    {
                        UserId = member.Id,
                        Points = -scoreToDeduct,
                        Type = PointType.Used,
                        Description = $"Converted {actualTicketsConverted} tickets from {scoreToDeduct} points",
                        IsUsed = true,
                        BookingId = newBooking.Id
                    });
                }

                // 9. Build success response
                var successResponse = new BookingConfirmSuccessDto
                {
                    BookingCode = newBooking.BookingCode,
                    MovieTitle = movie.Title,
                    CinemaRoom = showTime.Room?.RoomName ?? "Screen 1",
                    ShowDate = showTime.ShowDate?.ToString("dd/MM/yyyy") ?? "",
                    ShowTime = showTime.ShowDate?.ToString("HH:mm") ?? "",
                    Seats = seatDetails,

                    // Financial summary
                    SubTotal = totalPrice,
                    ScoreDiscount = scoreDiscount,
                    Total = finalTotal,
                    // Có thể bổ sung PromotionDiscount nếu muốn trả về

                    // Score usage details
                    ScoreUsed = actualTicketsConverted > 0,
                    TicketsConvertedFromScore = actualTicketsConverted,
                    ScoreDeducted = scoreToDeduct,
                    RemainingScore = remainingScore,

                    // Other info
                    PaymentMethod = request.PaymentMethod,
                    BookingDate = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"),
                    Message = actualTicketsConverted > 0 
                        ? $"Booking confirmed! {actualTicketsConverted} tickets converted from {scoreToDeduct} points." + (promotionDiscount > 0 ? $" Promotion discount: {promotionDiscount} ({discountPercent}%)" : "")
                        : "Booking confirmed successfully!" + (promotionDiscount > 0 ? $" Promotion discount: {promotionDiscount} ({discountPercent}%)" : "")
                };

                return SuccessResp.Ok(successResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking with score conversion");
                return ErrorResp.InternalServerError("Error processing booking confirmation");
            }
        }

        public async Task<IActionResult> GetBookingListAsync(BookingFilterDto filter)
        {
            try
            {
                var query = _context.Bookings
                    .Include(b => b.User)
                    .Include(b => b.ShowTime)
                        .ThenInclude(st => st.Movie)
                    .Include(b => b.ShowTime)
                        .ThenInclude(st => st.Room)
                    .Include(b => b.BookingDetails)
                        .ThenInclude(bd => bd.Seat)
                    .AsQueryable();

                // Apply filters
                if (filter.FromDate.HasValue)
                {
                    query = query.Where(b => b.BookingDate >= filter.FromDate.Value);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(b => b.BookingDate <= filter.ToDate.Value.AddDays(1));
                }

                if (!string.IsNullOrEmpty(filter.MovieTitle))
                {
                    query = query.Where(b => b.ShowTime.Movie.Title.Contains(filter.MovieTitle));
                }

                if (!string.IsNullOrEmpty(filter.BookingStatus))
                {
                    if (Enum.TryParse<BookingStatus>(filter.BookingStatus, out var status))
                    {
                        query = query.Where(b => b.Status == status);
                    }
                }

                if (!string.IsNullOrEmpty(filter.CustomerSearch))
                {
                    query = query.Where(b => 
                        b.FullName.Contains(filter.CustomerSearch) ||
                        b.PhoneNumber.Contains(filter.CustomerSearch) ||
                        b.Email.Contains(filter.CustomerSearch));
                }

                if (!string.IsNullOrEmpty(filter.BookingCode))
                {
                    query = query.Where(b => b.BookingCode.Contains(filter.BookingCode));
                }

                // Get total count
                var totalRecords = await query.CountAsync();

                // Apply sorting
                switch (filter.SortBy.ToLower())
                {
                    case "bookingdate":
                        query = filter.SortDirection.ToLower() == "asc" 
                            ? query.OrderBy(b => b.BookingDate)
                            : query.OrderByDescending(b => b.BookingDate);
                        break;
                    case "customername":
                        query = filter.SortDirection.ToLower() == "asc"
                            ? query.OrderBy(b => b.FullName)
                            : query.OrderByDescending(b => b.FullName);
                        break;
                    case "movietitle":
                        query = filter.SortDirection.ToLower() == "asc"
                            ? query.OrderBy(b => b.ShowTime.Movie.Title)
                            : query.OrderByDescending(b => b.ShowTime.Movie.Title);
                        break;
                    case "totalamount":
                        query = filter.SortDirection.ToLower() == "asc"
                            ? query.OrderBy(b => b.TotalPrice)
                            : query.OrderByDescending(b => b.TotalPrice);
                        break;
                    default:
                        query = query.OrderByDescending(b => b.BookingDate);
                        break;
                }

                // Apply pagination
                var bookings = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Map to DTOs
                var bookingDtos = bookings.Select(b => new BookingListDto
                {
                    Id = b.Id,
                    BookingCode = b.BookingCode,
                    CustomerName = b.FullName,
                    CustomerPhone = b.PhoneNumber,
                    CustomerEmail = b.Email,
                    MovieTitle = b.ShowTime?.Movie?.Title ?? "N/A",
                    CinemaRoom = b.ShowTime?.Room?.RoomName ?? "N/A",
                    ShowDate = b.ShowTime?.ShowDate ?? DateTime.MinValue,
                    ShowTime = b.ShowTime?.ShowDate?.TimeOfDay ?? TimeSpan.Zero,
                    SeatNumbers = string.Join(", ", b.BookingDetails.Select(bd => bd.Seat.SeatCode).OrderBy(s => s)),
                    TotalAmount = b.TotalPrice,
                    BookingStatus = b.Status.ToString(),
                    BookingDate = b.BookingDate,
                    PaymentMethod = "Cash", // Default for now
                    UsedPoints = (int)(b.PointsUsed ?? 0)
                }).ToList();

                var totalPages = (int)Math.Ceiling((double)totalRecords / filter.PageSize);

                var response = new BookingListResponseDto
                {
                    Bookings = bookingDtos,
                    TotalRecords = totalRecords,
                    CurrentPage = filter.Page,
                    TotalPages = totalPages,
                    PageSize = filter.PageSize
                };

                return SuccessResp.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking list");
                return ErrorResp.InternalServerError("Error retrieving booking list");
            }
        }

        public async Task<IActionResult> UpdateBookingStatusAsync(Guid bookingId, string newStatus)
        {
            try
            {
                var booking = await _bookingRepo.FindByIdAsync(bookingId);
                if (booking == null)
                {
                    return ErrorResp.NotFound("Booking not found");
                }

                if (Enum.TryParse<BookingStatus>(newStatus, out var status))
                {
                    booking.Status = status;
                    await _bookingRepo.UpdateAsync(booking);
                    
                    return SuccessResp.Ok(new { message = "Booking status updated successfully" });
                }
                else
                {
                    return ErrorResp.BadRequest("Invalid booking status");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status for booking {BookingId}", bookingId);
                return ErrorResp.InternalServerError("Error updating booking status");
            }
        }

        public async Task<IActionResult> CancelBookingAsync(Guid bookingId, string reason)
        {
            try
            {
                var booking = await _bookingRepo.FindByIdAsync(bookingId, "BookingDetails", "ShowTime");
                if (booking == null)
                {
                    return ErrorResp.NotFound("Booking not found");
                }

                // Check if booking can be cancelled (e.g., not already cancelled, showtime hasn't started yet)
                if (booking.Status == BookingStatus.Canceled)
                {
                    return ErrorResp.BadRequest("Booking is already cancelled");
                }

                if (booking.ShowTime?.ShowDate < DateTime.Now)
                {
                    return ErrorResp.BadRequest("Cannot cancel booking for past showtimes");
                }

                // Update booking status
                booking.Status = BookingStatus.Canceled;
                await _bookingRepo.UpdateAsync(booking);

                // Free up the seats
                foreach (var detail in booking.BookingDetails)
                {
                    var seat = await _seatRepository.GetByIdAsync(detail.SeatId);
                    if (seat != null)
                    {
                        seat.Status = SeatStatus.Available; // Make seat available again
                        await _seatRepository.UpdateSeatAsync(seat);
                    }
                }

                // If points were used, refund them
                if (booking.PointsUsed.HasValue && booking.PointsUsed > 0)
                {
                    var user = await _userRepo.FindByIdAsync(booking.UserId);
                    if (user != null)
                    {
                        user.Score += booking.PointsUsed.Value;
                        await _userRepo.UpdateAsync(user);

                        // Create point history record for refund
                        await _pointHistoryRepo.CreateAsync(new PointHistory
                        {
                            UserId = user.Id,
                            Points = booking.PointsUsed.Value,
                            Type = PointType.Earned,
                            Description = $"Refund for cancelled booking {booking.BookingCode}: {reason}",
                            BookingId = booking.Id
                        });
                    }
                }

                return SuccessResp.Ok(new { message = "Booking cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
                return ErrorResp.InternalServerError("Error cancelling booking");
            }
        }

        public async Task<IActionResult> GetUserBookingHistoryAsync(Guid userId)
        {
            try
            {
                var bookings = await _bookingRepo.WhereAsync(b => b.UserId == userId,
                    "ShowTime",
                    "ShowTime.Movie",
                    "ShowTime.Room",
                    "BookingDetails",
                    "BookingDetails.Seat");
                if (bookings == null || !bookings.Any())
                {
                    return ErrorResp.NotFound("No booking history found for this user.");
                }

                var bookingDtos = bookings.Select(b => new BookingHistoryItemDto
                {
                    BookingId = b.Id,
                    BookingCode = b.BookingCode,
                    MovieTitle = b.ShowTime?.Movie?.Title ?? "N/A",
                    CinemaRoom = b.ShowTime?.Room?.RoomName ?? "N/A",
                    ShowDate = b.ShowTime?.ShowDate ?? DateTime.MinValue,
                    ShowTime = b.ShowTime?.ShowDate?.TimeOfDay ?? TimeSpan.Zero,
                    Amount = b.TotalPrice,
                    Status = b.Status.ToString(),
                    BookingDate = b.BookingDate,
                    IsRefund = b.Status == BookingStatus.Canceled
                }).ToList();

                return SuccessResp.Ok(bookingDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user booking history for user {UserId}", userId);
                return ErrorResp.InternalServerError("Error retrieving user booking history");
            }
        }

        private async Task AwardPointsAndVouchersAsync(Guid userId, int seatsBooked, Guid bookingId, string bookingCode)
        {
            var user = await _userRepo.FindByIdAsync(userId);
            if(user == null) return;

            // 100 điểm mỗi booking (KHÔNG phải mỗi vé)
            var earned = 100;
            user.Score += earned;
            await _userRepo.UpdateAsync(user);

            // Lưu point history
            await _pointHistoryRepo.CreateAsync(new PointHistory
            {
                UserId = user.Id,
                Points = earned,
                Type = PointType.Earned,
                Description = $"Earned {earned} pts for booking {bookingCode}",
                BookingId = bookingId,
                IsUsed = false
            });

            // Check and grant vouchers
            var silverPromo = await _promotionRepo.FirstOrDefaultAsync(p => p.Title.ToLower().Contains("silver"));
            var goldPromo = await _promotionRepo.FirstOrDefaultAsync(p => p.Title.ToLower().Contains("gold"));

            if(silverPromo != null && user.Score >= 2000)
            {
                var exist = await _userPromotionRepo.FirstOrDefaultAsync(up => up.UserId == user.Id && up.PromotionId == silverPromo.Id);
                if(exist == null)
                {
                    await _userPromotionRepo.CreateAsync(new UserPromotion{ UserId = user.Id, PromotionId = silverPromo.Id });
                }
            }
            if(goldPromo != null && user.Score >= 5000)
            {
                var exist = await _userPromotionRepo.FirstOrDefaultAsync(up => up.UserId == user.Id && up.PromotionId == goldPromo.Id);
                if(exist == null)
                {
                    await _userPromotionRepo.CreateAsync(new UserPromotion{ UserId = user.Id, PromotionId = goldPromo.Id });
                }
            }
        }

        // API cho admin lấy booking theo bookingId (không kiểm tra userId)
        public async Task<IActionResult> GetAdminBookingDetailsAsync(Guid bookingId)
        {
            var booking = await _bookingRepo.FirstOrDefaultAsync(
                b => b.Id == bookingId,
                "User",
                "ShowTime.Movie",
                "ShowTime.Room",
                "BookingDetails.Seat"
            );

            if (booking == null)
            {
                return ErrorResp.NotFound("Booking not found");
            }

            var response = new
            {
                bookingId = booking.Id,
                bookingCode = booking.BookingCode,
                status = booking.Status.ToString(),
                movieTitle = booking.ShowTime?.Movie?.Title,
                cinemaRoom = booking.ShowTime?.Room?.RoomName,
                showDate = booking.ShowTime?.ShowDate?.ToString("dd/MM/yyyy"),
                showTime = booking.ShowTime?.ShowDate?.ToString("HH:mm"),
                seats = booking.BookingDetails?.Select(bd => bd.Seat?.SeatCode).ToList(),
                totalPrice = booking.TotalPrice,
                memberInfo = booking.User == null ? null : new
                {
                    memberId = booking.User.Id,
                    fullName = booking.User.FullName
                },
                bookingDate = booking.BookingDate.ToString("dd/MM/yyyy HH:mm:ss"),
            };

            return SuccessResp.Ok(response);
        }
    }
}
