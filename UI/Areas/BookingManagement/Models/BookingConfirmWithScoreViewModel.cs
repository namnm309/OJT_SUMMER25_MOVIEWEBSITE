using System.ComponentModel.DataAnnotations;

namespace UI.Areas.BookingManagement.Models
{



    public class BookingConfirmationDetailViewModel
    {

        public string BookingId { get; set; }
        public string MovieName { get; set; }
        public string Screen { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Seat { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }


        public string MemberId { get; set; }
        public string FullName { get; set; }
        public double MemberScore { get; set; }
        public string IdentityCard { get; set; }
        public string PhoneNumber { get; set; }


        public List<SeatDetailViewModel> SeatDetails { get; set; } = new List<SeatDetailViewModel>();
        public bool CanConvertScore { get; set; }
        public int MaxTicketsFromScore { get; set; }
        public double ScorePerTicket { get; set; }
    }




    public class SeatDetailViewModel
    {
        public Guid Id { get; set; }
        public string SeatCode { get; set; }
        public decimal Price { get; set; }
    }




    public class BookingConfirmWithScoreViewModel
    {
        [Required]
        public Guid ShowTimeId { get; set; }

        [Required]
        public List<Guid> SeatIds { get; set; } = new List<Guid>();

        [Required]
        public string MemberId { get; set; }
        

        public bool UseScoreConversion { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Số vé chuyển đổi phải lớn hơn hoặc bằng 0")]
        public int TicketsToConvert { get; set; } = 0;
        

        [Required]
        public string PaymentMethod { get; set; } = "cash";
        
        [Required]
        public string StaffId { get; set; }
        
        public string Notes { get; set; }
    }




    public class BookingConfirmSuccessViewModel
    {
        public string BookingCode { get; set; }
        public string MovieTitle { get; set; }
        public string CinemaRoom { get; set; }
        public string ShowDate { get; set; }
        public string ShowTime { get; set; }
        public List<SeatDetailViewModel> Seats { get; set; } = new List<SeatDetailViewModel>();
        

        public decimal SubTotal { get; set; }
        public decimal ScoreDiscount { get; set; }
        public decimal Total { get; set; }
        

        public bool ScoreUsed { get; set; }
        public int TicketsConvertedFromScore { get; set; }
        public double ScoreDeducted { get; set; }
        public double RemainingScore { get; set; }
        

        public string PaymentMethod { get; set; }
        public string BookingDate { get; set; }
        public string Message { get; set; }
    }
} 