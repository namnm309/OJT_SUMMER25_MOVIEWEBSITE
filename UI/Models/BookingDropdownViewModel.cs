using System;
using System.Collections.Generic;

namespace UI.Models
{

    public class BookingDropdownViewModel
    {
        public List<MovieViewModel> Movies { get; set; } = new List<MovieViewModel>();

        public List<DateTime>? Dates { get; set; }
        public List<ShowtimeDropdownDto>? Times { get; set; }

        // giữ giá trị đã chọn từ dropdown
        public string? SelectedMovieId { get; set; }
        public string? SelectedDate { get; set; }
        public string? SelectedShowtimeId { get; set; }
    }


}