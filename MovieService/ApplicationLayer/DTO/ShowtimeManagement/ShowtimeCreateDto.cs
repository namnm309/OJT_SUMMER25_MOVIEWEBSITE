using System;
using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.DTO.ShowtimeManagement
{
    public class ShowtimeCreateDto
    {
        [Required]
        public Guid MovieId { get; set; }

        [Required]
        public Guid CinemaRoomId { get; set; }

        [Required]
        public DateTime ShowDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Range(0, 1000000, ErrorMessage = "Giá vé phải từ 0 đến 1,000,000")]
        public decimal Price { get; set; }
    }
} 