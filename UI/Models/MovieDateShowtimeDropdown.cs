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

    // This DTO can be used for the ApiResponse data structure directly
    // since the API returns "data": [...]
    public class ApiResponseData<T>
    {
        public T Data { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }
}