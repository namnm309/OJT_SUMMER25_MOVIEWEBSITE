namespace UI.Models
{
    public class SeatDto
    {
        public Guid Id { get; set; }
        public string SeatCode { get; set; }
        public string SeatType { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public bool IsAvailable { get; set; }
        public decimal Price { get; set; }
    }

    public class SeatSelectionViewModel
    {
        public string RoomName { get; set; }
        public List<SeatDto> Seats { get; set; } = new List<SeatDto>();
        public List<Guid> SelectedSeatIds { get; set; } = new List<Guid>();
        public int SeatQuantity { get; set; } = 1;
        public Guid ShowtimeId { get; set; }
        public string MovieTitle { get; set; }
        public DateTime ShowDate { get; set; }
        public string ShowTime { get; set; }

        public string MovieDescription { get; set; }
        public string MovieDirector { get; set; }
        public string MovieActors { get; set; }
        public int MovieRunningTime { get; set; }
        public string MoviePrimaryImageUrl { get; set; }
        public List<string> MovieGenres { get; set; } = new List<string>();
    }

    public class SeatValidationResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public int SelectedSeatCount { get; set; }
    }

    public class ShowtimeDetailsDto
    {
        public Guid MovieId { get; set; }
        public Guid RoomId { get; set; }
        public DateTime ShowDate { get; set; }
    }
}
