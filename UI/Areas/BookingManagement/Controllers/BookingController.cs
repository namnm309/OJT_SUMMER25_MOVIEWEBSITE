using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UI.Areas.BookingManagement.Models;
using UI.Areas.BookingManagement.Services;

namespace UI.Areas.BookingManagement.Controllers
{
    [Area("BookingManagement")]
    [Authorize(Roles = "Member,Admin,Staff")]
    public class BookingController : Controller
    {
        private readonly IBookingManagementUIService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingManagementUIService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        // T7: Search Movies
        [HttpGet]
        public IActionResult SearchMovies(string searchTerm = "")
        {
            ViewData["Title"] = "Tìm kiếm phim";
            
            var model = new SearchMovieViewModel 
            { 
                SearchTerm = searchTerm,
                HasSearched = !string.IsNullOrEmpty(searchTerm)
            };

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Dummy data for search results
                model.Results = GetDummyMovies().Where(m => 
                    m.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    m.Genre.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();
                model.TotalResults = model.Results.Count;
            }

            return View(model);
        }

        // T8: Select Movie and Showtime
        [HttpGet]
        public IActionResult SelectMovie()
        {
            ViewData["Title"] = "Chọn phim và suất chiếu";
            
            var model = new SelectMovieViewModel
            {
                Movies = GetDummyMovies()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult GetShowDates(Guid movieId)
        {
            var dates = GetDummyShowDates();
            return Json(new { success = true, data = dates });
        }

        [HttpPost]
        public IActionResult GetShowTimes(Guid movieId, DateTime showDate)
        {
            var showTimes = GetDummyShowTimes(movieId, showDate);
            return Json(new { success = true, data = showTimes });
        }

        // T9: Select Seats
        [HttpGet]
        public IActionResult SelectSeat(Guid showTimeId)
        {
            ViewData["Title"] = "Chọn ghế";
            
            var model = GetDummySeatViewModel(showTimeId);
            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateSelectedSeats([FromBody] List<Guid> seatIds)
        {
            // Tính toán giá
            var dummySeats = GetDummySeats();
            var selectedSeats = dummySeats.Where(s => seatIds.Contains(s.Id)).ToList();
            var totalPrice = selectedSeats.Sum(s => s.Price);

            return Json(new { 
                success = true, 
                totalPrice = totalPrice,
                selectedCount = selectedSeats.Count,
                seats = selectedSeats.Select(s => new { 
                    id = s.Id, 
                    seatNumber = s.SeatNumber, 
                    price = s.Price,
                    type = s.Type.ToString()
                })
            });
        }

        // T10: Confirm Booking
        [HttpGet]
        public IActionResult ConfirmBooking(Guid showTimeId, string seatIds)
        {
            ViewData["Title"] = "Xác nhận đặt vé";
            
            var seatIdList = seatIds.Split(',').Select(Guid.Parse).ToList();
            var model = GetDummyConfirmViewModel(showTimeId, seatIdList);
            
            // Điền thông tin khách hàng từ Claims
            model.CustomerName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "";
            model.CustomerEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            model.CustomerPhone = User.FindFirst("Phone")?.Value ?? "0123456789";
            model.CustomerIdCard = User.FindFirst("IdentityCard")?.Value ?? "123456789";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmBooking(ConfirmBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Simulate booking confirmation
                var bookingId = Guid.NewGuid();
                var bookingCode = $"BK{DateTime.Now:yyyyMMddHHmmss}";

                TempData["BookingSuccess"] = true;
                TempData["BookingId"] = bookingId.ToString();
                TempData["BookingCode"] = bookingCode;

                return RedirectToAction("TicketInfo", new { bookingId = bookingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking");
                ModelState.AddModelError("", "Có lỗi xảy ra khi xác nhận đặt vé. Vui lòng thử lại.");
                return View(model);
            }
        }

        // T11: Ticket Information
        [HttpGet]
        public IActionResult TicketInfo(Guid bookingId)
        {
            ViewData["Title"] = "Thông tin vé";
            
            var model = GetDummyTicketInfo(bookingId);
            
            // Kiểm tra xem có booking success từ TempData không
            if (TempData["BookingSuccess"] != null)
            {
                ViewBag.IsNewBooking = true;
                ViewBag.BookingCode = TempData["BookingCode"];
            }

            return View(model);
        }

        // Helper Methods for Dummy Data
        private List<MovieOption> GetDummyMovies()
        {
            return new List<MovieOption>
            {
                new MovieOption
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Title = "Avengers: Endgame",
                    Poster = "https://image.tmdb.org/t/p/w500/or06FN3Dka5tukK1e9sl16pB3iy.jpg",
                    Duration = 181,
                    Genre = "Hành động, Khoa học viễn tưởng",
                    Price = 85000,
                    IsAvailable = true
                },
                new MovieOption
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Title = "Spider-Man: No Way Home",
                    Poster = "https://image.tmdb.org/t/p/w500/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg",
                    Duration = 148,
                    Genre = "Hành động, Phiêu lưu",
                    Price = 90000,
                    IsAvailable = true
                },
                new MovieOption
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Title = "The Batman",
                    Poster = "https://image.tmdb.org/t/p/w500/b0PlSFdDwbyK0cf5RxwDpaOJQvQ.jpg",
                    Duration = 176,
                    Genre = "Hành động, Tội phạm",
                    Price = 95000,
                    IsAvailable = true
                },
                new MovieOption
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Title = "Top Gun: Maverick",
                    Poster = "https://image.tmdb.org/t/p/w500/62HCnUTziyWcpDaBO2i1DX17ljH.jpg",
                    Duration = 130,
                    Genre = "Hành động, Drama",
                    Price = 80000,
                    IsAvailable = true
                }
            };
        }

        private List<DateTime> GetDummyShowDates()
        {
            var today = DateTime.Today;
            return Enumerable.Range(0, 7).Select(i => today.AddDays(i)).ToList();
        }

        private List<ShowTimeOption> GetDummyShowTimes(Guid movieId, DateTime showDate)
        {
            return new List<ShowTimeOption>
            {
                new ShowTimeOption
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    MovieId = movieId,
                    CinemaRoomId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    CinemaRoomName = "Phòng 1",
                    StartTime = showDate.Add(new TimeSpan(9, 0, 0)),
                    EndTime = showDate.Add(new TimeSpan(12, 0, 0)),
                    Price = 85000,
                    AvailableSeats = 45,
                    TotalSeats = 60
                },
                new ShowTimeOption
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    MovieId = movieId,
                    CinemaRoomId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    CinemaRoomName = "Phòng 2",
                    StartTime = showDate.Add(new TimeSpan(14, 30, 0)),
                    EndTime = showDate.Add(new TimeSpan(17, 30, 0)),
                    Price = 90000,
                    AvailableSeats = 32,
                    TotalSeats = 50
                },
                new ShowTimeOption
                {
                    Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                    MovieId = movieId,
                    CinemaRoomId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    CinemaRoomName = "Phòng 1",
                    StartTime = showDate.Add(new TimeSpan(20, 0, 0)),
                    EndTime = showDate.Add(new TimeSpan(23, 0, 0)),
                    Price = 95000,
                    AvailableSeats = 28,
                    TotalSeats = 60
                }
            };
        }

        private SelectSeatViewModel GetDummySeatViewModel(Guid showTimeId)
        {
            var movie = GetDummyMovies().First();
            var showTime = DateTime.Today.Add(new TimeSpan(20, 0, 0));

            return new SelectSeatViewModel
            {
                ShowTimeId = showTimeId,
                MovieTitle = movie.Title,
                CinemaRoom = "Phòng 1",
                ShowTime = showTime,
                Seats = GetDummySeats(),
                RegularSeatPrice = 85000,
                VipSeatPrice = 120000
            };
        }

        private List<SeatInfo> GetDummySeats()
        {
            var seats = new List<SeatInfo>();
            var random = new Random();
            
            // Tạo 8 hàng ghế (A-H), mỗi hàng 10 ghế
            for (int row = 1; row <= 8; row++)
            {
                var rowLetter = ((char)('A' + row - 1)).ToString();
                for (int col = 1; col <= 10; col++)
                {
                    var isVip = row >= 6; // Hàng F, G, H là VIP
                    var isOccupied = random.Next(0, 100) < 20; // 20% ghế đã có người
                    
                    seats.Add(new SeatInfo
                    {
                        Id = Guid.NewGuid(),
                        SeatNumber = $"{rowLetter}{col:D2}",
                        Row = row,
                        Column = col,
                        Type = isVip ? SeatType.VIP : SeatType.Regular,
                        IsOccupied = isOccupied,
                        Price = isVip ? 120000 : 85000
                    });
                }
            }
            
            return seats;
        }

        private ConfirmBookingViewModel GetDummyConfirmViewModel(Guid showTimeId, List<Guid> seatIds)
        {
            var movie = GetDummyMovies().First();
            var seats = GetDummySeats().Where(s => seatIds.Contains(s.Id)).ToList();
            var totalPrice = seats.Sum(s => s.Price);

            return new ConfirmBookingViewModel
            {
                ShowTimeId = showTimeId,
                MovieTitle = movie.Title,
                MoviePoster = movie.Poster,
                CinemaRoom = "Phòng 1",
                ShowDate = DateTime.Today.AddDays(1),
                ShowTime = new TimeSpan(20, 0, 0),
                SelectedSeats = seats.Select(s => s.SeatNumber).ToList(),
                SelectedSeatIds = seatIds,
                PricePerTicket = seats.Count > 0 ? seats.Average(s => s.Price) : 85000,
                TotalPrice = totalPrice,
                TotalSeats = seats.Count,
                AvailablePoints = 500, // Dummy points
                FinalPrice = totalPrice
            };
        }

        private TicketInfoViewModel GetDummyTicketInfo(Guid bookingId)
        {
            var movie = GetDummyMovies().First();
            
            return new TicketInfoViewModel
            {
                BookingId = bookingId,
                BookingCode = TempData["BookingCode"]?.ToString() ?? $"BK{DateTime.Now:yyyyMMddHHmmss}",
                MovieTitle = movie.Title,
                MoviePoster = movie.Poster,
                CinemaRoom = "Phòng 1",
                ShowDate = DateTime.Today.AddDays(1),
                ShowTime = new TimeSpan(20, 0, 0),
                SeatNumbers = new List<string> { "F05", "F06" },
                TotalPrice = 240000,
                BookingTime = DateTime.Now,
                Status = BookingStatus.Confirmed,
                CustomerName = "Nguyễn Văn A",
                CustomerPhone = "0123456789",
                CustomerEmail = "nguyenvana@email.com",
                PointsEarned = 24 // 10% của tổng tiền
            };
        }
    }
} 