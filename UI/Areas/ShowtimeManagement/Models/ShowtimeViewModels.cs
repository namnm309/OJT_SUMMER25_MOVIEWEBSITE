using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UI.Areas.ShowtimeManagement.Models
{
    public class ShowtimePageViewModel
    {
        public WeekInfo CurrentWeek { get; set; }
        public List<ShowtimeDto> Showtimes { get; set; }
        public List<MovieDto> Movies { get; set; }
        public List<CinemaRoomDto> CinemaRooms { get; set; }
    }

    public class WeekInfo
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int WeekNumber { get; set; }
        
        public List<DateTime> GetDaysOfWeek()
        {
            var days = new List<DateTime>();
            for (var date = StartDate; date <= EndDate; date = date.AddDays(1))
            {
                days.Add(date);
            }
            return days;
        }
    }

    public class ShowtimeDto
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public string MovieTitle { get; set; }
        public string MoviePoster { get; set; }
        public int MovieDuration { get; set; }
        public Guid CinemaRoomId { get; set; }
        public string CinemaRoomName { get; set; }
        public int TotalSeats { get; set; }
        public int BookedSeats { get; set; }
        public DateTime ShowDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }

    public class MovieDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int RunningTime { get; set; }
        public string PrimaryImageUrl { get; set; }
        public string Status { get; set; }
    }

    public class CinemaRoomDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int TotalSeats { get; set; }
        public string RoomType { get; set; }
    }

    public class CreateShowtimeViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn phim")]
        public Guid MovieId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng chiếu")]
        public Guid CinemaRoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày chiếu")]
        [DataType(DataType.Date)]
        public DateTime ShowDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giờ chiếu")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá vé")]
        [Range(0, 1000000, ErrorMessage = "Giá vé phải từ 0 đến 1,000,000")]
        public decimal Price { get; set; }

        public List<MovieDto> Movies { get; set; }
        public List<CinemaRoomDto> CinemaRooms { get; set; }
    }

    public class EditShowtimeViewModel : CreateShowtimeViewModel
    {
        public Guid Id { get; set; }
    }

    public class ShowtimeFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? MovieId { get; set; }
        public Guid? CinemaRoomId { get; set; }
        public string Status { get; set; }
    }

    public class ShowtimeCalendarEvent
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string ResourceId { get; set; }
        public string BackgroundColor { get; set; }
        public string BorderColor { get; set; }
        public string TextColor { get; set; }
        public Dictionary<string, object> ExtendedProps { get; set; }
    }
} 