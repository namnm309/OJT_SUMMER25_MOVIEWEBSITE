namespace ApplicationLayer.DTO.ShowtimeManagement
{
    public class ShowtimeConflictDto
    {
        public bool HasConflict { get; set; }
        public string ConflictMessage { get; set; }
        public List<ConflictingShowtimeInfo> ConflictingShowtimes { get; set; } = new List<ConflictingShowtimeInfo>();
    }

    public class ConflictingShowtimeInfo
    {
        public Guid Id { get; set; }
        public string MovieTitle { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ConflictType { get; set; } // "start_time", "end_time", "overlap", "encompass"
    }
} 