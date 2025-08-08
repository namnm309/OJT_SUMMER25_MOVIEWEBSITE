namespace UI.Models
{
    public class MovieDropdownDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }

    public class ShowtimeDropdownDto
    {
        public Guid Id { get; set; }
        public string Time { get; set; }
    }



    public class ApiResponseData<T>
    {
        public T Data { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }
}