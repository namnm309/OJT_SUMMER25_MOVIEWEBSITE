using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization; // Thêm dòng này
using UI.Areas.BookingManagement.Models;
using UI.Areas.BookingManagement.Services;
using UI.Services;
using UI.Models;

namespace UI.Areas.BookingManagement.Controllers
{
    [Area("BookingManagement")]
    [Authorize(Roles = "Member,Admin,Staff")]
    public class BookingController : Controller
    {
        private readonly IBookingManagementUIService _bookingService;
        private readonly ILogger<BookingController> _logger;
        private readonly IApiService _apiService;

        public BookingController(IBookingManagementUIService bookingService, ILogger<BookingController> logger, IApiService apiService)
        {
            _bookingService = bookingService;
            _logger = logger;
            _apiService = apiService;
        }

        // T7: Search Movies
        [HttpGet]
        public async Task<IActionResult> SearchMovies(string searchTerm = "")
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
        public async Task<IActionResult> SelectMovie()
        {
            var viewModel = new BookingDropdownViewModel();

            try
            {
                var result = await _apiService.GetAsync<JsonElement>("/api/v1/movie/View");

                if (result.Success && result.Data.TryGetProperty("data", out var dataProp))
                {
                    // Log raw response để debug
                    _logger.LogInformation("Raw API response: {Response}", dataProp.GetRawText());

                    var movies = JsonSerializer.Deserialize<List<MovieViewModel>>(dataProp.GetRawText(), new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                        // Xóa dòng Converters
                    });

                    // Kiểm tra và log từng movie ID
                    if (movies != null)
                    {
                        foreach (var movie in movies)
                        {
                            _logger.LogInformation("Movie: {Title}, ID: '{Id}', Length: {Length}",
                                movie.Title, movie.Id, movie.Id?.Length ?? 0);
                        }
                    }

                    viewModel.Movies = movies ?? new List<MovieViewModel>();
                }
                else
                {
                    _logger.LogWarning("API call failed or returned no data");
                    viewModel.Movies = new List<MovieViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading movies for booking");
                viewModel.Movies = new List<MovieViewModel>();
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetMovieDates(string movieId)
        {
            try
            {
                var result = await _apiService.GetAsync<dynamic>($"/api/v1/booking-ticket/dropdown/movies/{movieId}/dates");

                if (result.Success)
                {
                    return Json(new { success = true, data = result.Data });
                }

                return Json(new { success = false, message = "Không thể lấy ngày chiếu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie dates for movieId: {MovieId}", movieId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy ngày chiếu" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetShowDates([FromBody] Guid movieId)
        {
            try
            {
                var response = await _bookingService.GetShowDatesAsync(movieId);

                if (response?.Success == true && response.Data != null)
                {
                    // Convert dynamic data to DateTime list
                    var dates = new List<DateTime>();

                    if (response.Data is IEnumerable<object> dateObjects)
                    {
                        foreach (var dateObj in dateObjects)
                        {
                            if (DateTime.TryParse(dateObj.ToString(), out DateTime date))
                            {
                                dates.Add(date);
                            }
                        }
                    }

                    var formattedDates = dates.Select(d => d.ToString("yyyy-MM-dd")).ToList();
                    return Json(new { success = true, data = formattedDates });
                }

                return Json(new { success = false, message = "Không thể tải ngày chiếu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading show dates for movie {MovieId}", movieId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải ngày chiếu" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetShowTimes([FromBody] GetShowTimesRequest request)
        {
            try
            {
                var response = await _bookingService.GetShowTimesAsync(request.MovieId, request.ShowDate);

                if (response?.Success == true && response.Data != null)
                {
                    // Convert dynamic data to ShowTimeOption list
                    var showTimes = ConvertToShowTimeOptions(response.Data);
                    return Json(new { success = true, data = showTimes });
                }

                return Json(new { success = false, message = "Không thể tải suất chiếu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading show times for movie {MovieId} on {ShowDate}", request.MovieId, request.ShowDate);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải suất chiếu" });
            }
        }

        // Helper methods to convert API response to view models
        private List<MovieOption> ConvertToMovieOptions(dynamic moviesData)
        {
            var movies = new List<MovieOption>();

            try
            {
                if (moviesData is IEnumerable<object> movieList)
                {
                    foreach (dynamic movie in movieList)
                    {
                        movies.Add(new MovieOption
                        {
                            Id = Guid.TryParse(movie.id?.ToString(), out Guid id) ? id : Guid.NewGuid(),
                            Title = movie.name?.ToString() ?? "Unknown Movie",  // Đổi từ movie.title thành movie.name
                            Poster = movie.image?.ToString() ?? "/images/default-movie.jpg",  // Đổi từ movie.poster thành movie.image
                            Duration = int.TryParse(movie.duration?.ToString(), out int duration) ? duration : 120,
                            Genre = movie.genre?.ToString() ?? "Unknown Genre",
                            Price = decimal.TryParse(movie.price?.ToString(), out decimal price) ? price : 85000,
                            IsAvailable = bool.TryParse(movie.isAvailable?.ToString(), out bool available) ? available : true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting movies data");
            }

            return movies;
        }

        private List<ShowTimeOption> ConvertToShowTimeOptions(dynamic showTimesData)
        {
            var showTimes = new List<ShowTimeOption>();

            try
            {
                if (showTimesData is IEnumerable<object> showTimeList)
                {
                    foreach (dynamic showTime in showTimeList)
                    {
                        showTimes.Add(new ShowTimeOption
                        {
                            Id = Guid.TryParse(showTime.id?.ToString(), out Guid id) ? id : Guid.NewGuid(),
                            MovieId = Guid.TryParse(showTime.movieId?.ToString(), out Guid movieId) ? movieId : Guid.Empty,
                            CinemaRoomId = Guid.TryParse(showTime.cinemaRoomId?.ToString(), out Guid roomId) ? roomId : Guid.Empty,
                            CinemaRoomName = showTime.cinemaRoomName?.ToString() ?? "Unknown Room",
                            StartTime = DateTime.TryParse(showTime.startTime?.ToString(), out DateTime startTime) ? startTime : DateTime.Now,
                            EndTime = DateTime.TryParse(showTime.endTime?.ToString(), out DateTime endTime) ? endTime : DateTime.Now.AddHours(2),
                            Price = decimal.TryParse(showTime.price?.ToString(), out decimal price) ? price : 85000,
                            AvailableSeats = int.TryParse(showTime.availableSeats?.ToString(), out int available) ? available : 0,
                            TotalSeats = int.TryParse(showTime.totalSeats?.ToString(), out int total) ? total : 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting show times data");
            }

            return showTimes;
        }

        // T9: Select Seats
        [HttpGet]
        public async Task<IActionResult> SelectSeat(Guid showTimeId)
        {
            ViewData["Title"] = "Chọn ghế";

            var model = GetDummySeatViewModel(showTimeId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSelectedSeats([FromBody] List<Guid> seatIds)
        {
            // Tính toán giá
            var dummySeats = GetDummySeats();
            var selectedSeats = dummySeats.Where(s => seatIds.Contains(s.Id)).ToList();
            var totalPrice = selectedSeats.Sum(s => s.Price);

            return Json(new
            {
                success = true,
                totalPrice = totalPrice,
                selectedCount = selectedSeats.Count,
                seats = selectedSeats.Select(s => new
                {
                    id = s.Id,
                    seatNumber = s.SeatNumber,
                    price = s.Price,
                    type = s.Type.ToString()
                })
            });
        }

        // T10: Confirm Booking
        [HttpGet]
        public async Task<IActionResult> ConfirmBooking(Guid showTimeId, string seatIds)
        {
            ViewData["Title"] = "Xác nhận đặt vé";

            // Tạo model với thông tin cơ bản
            var model = new ConfirmBookingViewModel
            {
                ShowTimeId = showTimeId,
                SelectedSeatIds = !string.IsNullOrEmpty(seatIds)
                    ? seatIds.Split(',').Select(Guid.Parse).ToList()
                    : new List<Guid>(),
                // Thông tin sẽ được load động từ JavaScript
                MovieTitle = "Đang tải...",
                MoviePoster = "/images/placeholder-movie.jpg",
                CinemaRoom = "Đang tải...",
                ShowDate = DateTime.Today,
                ShowTime = TimeSpan.Zero,
                SelectedSeats = new List<string>(),
                TotalPrice = 0,
                TotalSeats = 0,
                AvailablePoints = 500, // Lấy từ user claims hoặc API
                FinalPrice = 0
            };

            // Điền thông tin khách hàng từ Claims
            model.CustomerName = User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "";
            model.CustomerEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            model.CustomerPhone = User.FindFirst("Phone")?.Value ?? "0123456789";
            model.CustomerIdCard = User.FindFirst("IdentityCard")?.Value ?? "123456789";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessBooking([FromBody] ProcessBookingRequest request)
        {
            try
            {
                // Lấy userId từ localStorage hoặc claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng" });
                }

                // Tạo request body theo format API
                var apiRequest = new
                {
                    showtimeId = request.ShowtimeId,
                    seatIds = request.SeatIds,
                    totalPrice = request.TotalPrice,
                    userId = userId,
                    fullName = request.FullName,
                    email = request.Email,
                    identityCard = request.IdentityCard,
                    phoneNumber = request.PhoneNumber
                };

                // Gọi API confirm-user-booking-v2
                var result = await _apiService.PostAsync<dynamic>(
                    "/api/v1/booking-ticket/confirm-user-booking-v2",
                    apiRequest
                );

                if (result.Success)
                {
                    // Lưu thông tin booking vào TempData để hiển thị ở trang kết quả
                    TempData["BookingResult"] = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Success = true,
                        Data = result.Data,
                        BookingInfo = apiRequest
                    });

                    return Json(new { success = true, redirectUrl = Url.Action("BookingResult") });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Có lỗi xảy ra khi đặt vé" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing booking");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xử lý đặt vé" });
            }
        }

        [HttpGet]
        public IActionResult BookingResult()
        {
            ViewData["Title"] = "Kết Quả Đặt Vé";

            var bookingResultJson = TempData["BookingResult"] as string;
            if (string.IsNullOrEmpty(bookingResultJson))
            {
                return RedirectToAction("SelectMovie");
            }

            var bookingResult = System.Text.Json.JsonSerializer.Deserialize<BookingResultViewModel>(bookingResultJson);
            return View(bookingResult);
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