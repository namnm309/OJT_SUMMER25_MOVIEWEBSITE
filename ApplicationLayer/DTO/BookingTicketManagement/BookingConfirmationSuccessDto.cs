using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class BookingConfirmationSuccessDto
    {
        public string BookingCode { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaRoomName { get; set; }
        public string ShowDate { get; set; }
        public string ShowTime { get; set; }
        public List<SeatBookingDto> BookedSeats { get; set; } = new List<SeatBookingDto>();
        public decimal TotalPrice { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string IdentityCard { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class SeatBookingDto
    {
        public Guid SeatId { get; set; }
        public string SeatCode { get; set; }
        // Có thể thêm loại ghế, giá ghế nếu cần
    }
}
