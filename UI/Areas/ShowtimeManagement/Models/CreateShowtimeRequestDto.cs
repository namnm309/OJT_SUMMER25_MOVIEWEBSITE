using System;
using System.ComponentModel.DataAnnotations;

namespace UI.Areas.ShowtimeManagement.Models
{
    public class CreateShowtimeRequestDto
    {
        [Required]
        public Guid MovieId { get; set; }
        [Required]
        public Guid CinemaRoomId { get; set; }
        [Required]
        public DateTime ShowDate { get; set; }
        [Required]
        public string StartTime { get; set; } = string.Empty; // HH:mm hoặc HH:mm:ss
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
} 