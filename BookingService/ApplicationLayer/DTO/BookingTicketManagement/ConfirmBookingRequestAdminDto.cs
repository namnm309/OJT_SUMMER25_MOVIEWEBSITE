using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DTO.BookingTicketManagement
{
    public class SeatSelectionDto
    {
        public Guid Id { get; set; }
        public string SeatCode { get; set; }
        public decimal Price { get; set; }
    }

    public class ValidateSeatsRequestDto
    {
        public Guid ShowTimeId { get; set; }
        public List<Guid> SeatIds { get; set; }
        public int SeatCount { get; set; }
    }

    public class CheckMemberRequestDto
    {
        public string? MemberId { get; set; }
        public string? IdentityNumber { get; set; }
    }

    public class MemberInfoDto
    {

        public string MemberId { get; set; }
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string PhoneNumber { get; set; }
        public double Points { get; set; }
    }

    public class ConfirmBookingRequestAdminDto
    {
        public Guid ShowTimeId { get; set; }
        public List<Guid> SeatIds { get; set; }
        public string? MemberId { get; set; }
        public int? ConvertedTickets { get; set; }
        public double? PointsUsed { get; set; }
        public string PaymentMethod { get; set; }
        public string StaffId { get; set; }
    }

    public class BookingConfirmationDto
    {
        public string BookingCode { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaRoom { get; set; }
        public string ShowDate { get; set; }
        public string ShowTime { get; set; }
        public List<SeatSelectionDto> Seats { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public double PointsUsed { get; set; }
        public double RemainingPoints { get; set; }
        public string PaymentMethod { get; set; }
        public string BookingDate { get; set; }
    }

    public class CreateMemberAccountDto
    {
        public string FullName { get; set; }
        public string IdentityCard { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }
}
