using Application.ResponseCode;
using ApplicationLayer.DTO.BookingTicketManagement;
using ApplicationLayer.DTO.UserManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using DomainLayer.Exceptions;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationLayer.Services.BookingTicketManagement
{
    public interface IBookingTicketService
    {
        Task<IActionResult> GetAvailableMovies(); //Lấy danh sách phim
        Task<IActionResult> GetShowDatesByMovie(Guid movieId); //Lấy danh sách ngày chiếu cho phim
        Task<IActionResult> GetShowTimesByMovieAndDate(Guid movieId, DateTime selectedDate); //Lấy các giờ chiếu trong ngày
        Task<IActionResult> ConfirmUserBooking(ConfirmBookingRequestDto request);


        Task<IActionResult> CheckMember(CheckMemberRequestDto request);

        Task<(bool Success, String message)> CreateMemberAccount(CreateMemberAccountDto request);
        Task<IActionResult> ConfirmAdminBooking(ConfirmBookingRequestAdminDto request);
        Task<IActionResult> GetBookingDetails(string bookingCode);
        Task<IActionResult> ConfirmBooking(ConfirmBookingRequestDto request);
        Task<IActionResult> GetBookingDetailsAsync(Guid bookingId, Guid userId);

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
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IMailService _mailService;
        private readonly IBookingRepository _bookingRepository;

        public BookingTicketService(
            IUserRepository userRepository,
            IGenericRepository<Movie> movieRepo,
            IGenericRepository<ShowTime> showtimeRepo,
            IGenericRepository<Booking> bookingRepo,
            IGenericRepository<BookingDetail> bookingDetailRepo,
            IGenericRepository<Users> userRepo,
            IGenericRepository<PointHistory> pointHistoryRepo,
            ISeatRepository seatRepository,
            IMapper mapper,
            IMailService mailService)
            IGenericRepository<Booking> bookingRepo, // Thêm
            IGenericRepository<BookingDetail> bookingDetailRepo, // Thêm
            ISeatRepository seatRepository, // Thêm
            IMapper mapper,
            IBookingRepository bookingRepository)
        {
            _userRepository = userRepository;
            _movieRepo = movieRepo;
            _showtimeRepo = showtimeRepo;
            _bookingRepo = bookingRepo;
            _bookingDetailRepo = bookingDetailRepo;
            _userRepo = userRepo;
            _pointHistoryRepo = pointHistoryRepo;
            _seatRepository = seatRepository;
            _mapper = mapper;
            _mailService = mailService;
            _bookingRepository = bookingRepository;
        }



        public async Task<IActionResult> GetAvailableMovies()
        {
            var movie = await _movieRepo.WhereAsync(m => m.Status == MovieStatus.NowShowing);
            if (movie == null)
                return ErrorResp.NotFound("Movie Not Found");

            var result = _mapper.Map<List<MovieDropdownDto>>(movie);

            return SuccessResp.Ok(result);
        }

        public async Task<IActionResult> GetShowDatesByMovie(Guid movieId)
        {
            var showtimes = await _showtimeRepo.WhereAsync(s => s.MovieId == movieId);
            if (showtimes == null || !showtimes.Any())
            {
                return ErrorResp.NotFound("No show dates found for this movie.");
            }

            var dates = showtimes
                .Where(s => s.ShowDate.HasValue)
                .Select(s => s.ShowDate.Value.Date)
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
            var showtime = await _showtimeRepo.WhereAsync(s =>
                s.MovieId == movieId &&
                s.ShowDate.HasValue &&
                s.ShowDate.Value.Date == selectedDate.Date);

            var timeList = showtime
                .Select(s => new
                {
                    s.Id,
                    Time = s.ShowDate.Value.ToString("HH:mm")
                }).ToList();

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
                    seat.IsActive = false;
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

                return SuccessResp.Ok(successResponse);
            }
            catch (NotFoundException ex)
            {
                // AC-05: Nếu quá trình gửi thất bại...
                return ErrorResp.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // AC-05: Nếu quá trình gửi thất bại. Vui lòng thử lại sau.
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
                    FullName = "",
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
    }
}
