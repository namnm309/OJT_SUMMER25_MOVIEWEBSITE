namespace UI.Models
{
    public class MovieDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Director { get; set; }
        public string Actors { get; set; }
        public int RunningTime { get; set; }
        public string PrimaryImageUrl { get; set; }
        public List<GenreDto> Genres { get; set; }
    }

    public class GenreDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
