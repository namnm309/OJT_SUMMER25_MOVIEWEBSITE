using System;

namespace ApplicationLayer.DTO.ShowtimeManagement
{
    public class CinemaRoomDropdownDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public bool IsActive { get; set; }
    }
}