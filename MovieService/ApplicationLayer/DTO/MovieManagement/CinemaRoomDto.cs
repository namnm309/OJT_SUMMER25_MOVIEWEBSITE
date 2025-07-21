using System;

namespace ApplicationLayer.DTO.MovieManagement
{
    public class CinemaRoomDto
    {
        public Guid Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public bool IsActive { get; set; }
    }
} 