using System;

namespace ApplicationLayer.DTO.ShowtimeManagement
{
    public class ShowtimeListDto
    {
        public Guid Id { get; set; }
        public Guid MovieId { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string MoviePoster { get; set; } = string.Empty;
        public int MovieDuration { get; set; }
        public Guid CinemaRoomId { get; set; }
        public string CinemaRoomName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int BookedSeats { get; set; }
        public DateTime ShowDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
} 