using System;
using System.Collections.Generic;

namespace UI.Models
{
    // ViewModels/BookingDropdownViewModel.cs
    public class BookingDropdownViewModel
    {
       
        public List<MovieDropdownDto>? Movies { get; set; }

       
        public List<DateTime>? Dates { get; set; }
        public List<ShowtimeDropdownDto>? Times { get; set; }

        // Các thuộc tính để giữ giá trị đã chọn từ dropdown
        public string? SelectedMovieId { get; set; }
        public string? SelectedDate { get; set; }
        public string? SelectedShowtimeId { get; set; } 
    }

  
}