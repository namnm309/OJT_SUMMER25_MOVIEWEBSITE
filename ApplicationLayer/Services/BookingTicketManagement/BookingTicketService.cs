using Application.ResponseCode;
using ApplicationLayer.DTO.BookingTicketManagement;
using AutoMapper;
using DomainLayer.Entities;
using DomainLayer.Enum;
using DomainLayer.Exceptions;
using InfrastructureLayer.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.BookingTicketManagement
{
    public interface IBookingTicketService
    {
        Task<IActionResult> GetAvailableMovies(); //Lấy danh sách phim
        Task<IActionResult> GetShowDatesByMovie(Guid movieId); //Lấy danh sách ngày chiếu cho phim
        Task<IActionResult> GetShowTimesByMovieAndDate(Guid movieId, DateTime selectedDate); //Lấy các giờ chiếu trong ngày
        Task<IActionResult> ConfirmBooking(ConfirmBookingRequestDto request);
    }
    public class BookingTicketService : IBookingTicketService
    {
        private readonly IGenericRepository<Movie> _movieRepo;
        private readonly IGenericRepository<ShowTime> _showtimeRepo;
        private readonly IGenericRepository<Booking> _bookingRepo; // Thêm
        private readonly IGenericRepository<BookingDetail> _bookingDetailRepo; // Thêm
        private readonly ISeatRepository _seatRepository; // Thêm để truy cập logic ghế đã có
        private readonly IMapper _mapper;

        public BookingTicketService(
            IGenericRepository<Movie> movieRepo,
            IGenericRepository<ShowTime> showtimeRepo,
            IGenericRepository<Booking> bookingRepo, // Thêm
            IGenericRepository<BookingDetail> bookingDetailRepo, // Thêm
            ISeatRepository seatRepository, // Thêm
            IMapper mapper)
        {
            _movieRepo = movieRepo;
            _showtimeRepo = showtimeRepo;
            _bookingRepo = bookingRepo; // Gán
            _bookingDetailRepo = bookingDetailRepo; // Gán
            _seatRepository = seatRepository; // Gán
            _mapper = mapper;
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
            var showtime = await _showtimeRepo.WhereAsync(s => s.MovieId == movieId);
            if (showtime == null)
                return ErrorResp.NotFound("Not Found");

            var date = showtime
                .Where(s => s.ShowDate.HasValue)
                .Select(s => s.ShowDate.Value.Date)
                .Distinct() // loại bỏ giá trị trùng lặp trong danh sách
                .OrderBy(d => d) //sắp xếp danh sách tăng dần
                .ToList();

            return SuccessResp.Ok(date);
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

        public async Task<IActionResult> ConfirmBooking(ConfirmBookingRequestDto request)
        {
            try
            {
                // AC-02: Tất cả các chi tiết đặt vé phải dựa trên các lựa chọn trước đó và dữ liệu tài khoản của người dùng.
                // 1. Lấy thông tin ShowTime và Room
                var showTime = await _showtimeRepo.FoundOrThrowAsync(request.ShowtimeId, "ShowTime not found.");
                var movie = await _movieRepo.FoundOrThrowAsync(showTime.MovieId, "Movie not found.");
                // Bạn có thể lấy thông tin User từ UserId trong request, hoặc từ context nếu đã xác thực
                // Ví dụ: var user = await _userRepo.FoundOrThrowAsync(request.UserId, "User not found.");

                // 2. Validate ghế một lần nữa để đảm bảo không có ghế nào bị đặt trong lúc chờ xác nhận
                // (Lưu ý: Bạn đã có ValidateSelectedSeats trong SeatService, nhưng nó cập nhật IsActive của ghế.
                //  Chức năng này nên chỉ để kiểm tra lại, và việc cập nhật IsActive nên diễn ra sau khi booking thành công.)
                //  Hoặc bạn có thể sử dụng một cơ chế khóa/đặt chỗ tạm thời.
                //  Hiện tại, hàm ValidateSelectedSeats của bạn đã thay đổi IsActive của ghế.
                //  Nếu IsActive được thay đổi trong bước ValidateSelectedSeats, thì ở đây chỉ cần kiểm tra lại.
                //  Nếu IsActive chưa được thay đổi, thì cần cơ chế để "khóa" ghế trước khi xác nhận cuối cùng.

                // Kiểm tra lại các ghế đã chọn có còn trống không
                var bookedSeatIdsForShowTime = await _seatRepository.GetBookedSeatIdsForShowTimeAsync(request.ShowtimeId);
                foreach (var seatId in request.SeatIds)
                {
                    if (bookedSeatIdsForShowTime.Contains(seatId))
                    {
                        // AC-05: Nếu quá trình gửi thất bại...
                        return ErrorResp.BadRequest($"Seat with ID {seatId} is already booked.");
                    }
                    // Kiểm tra IsActive của ghế (đảm bảo ghế không bị vô hiệu hóa vì lý do khác)
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
    }
}

