using System.Net;

namespace UI.Services
{
    public interface IApiService
    {
        // GET request với response type T
        Task<ApiResponse<T>> GetAsync<T>(string endpoint);

        // POST request với data và response type T
        Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null);

        // PUT request với data và response type T
        Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null);

        // PATCH request với data và response type T
        Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null);

        // DELETE request
        Task<ApiResponse<bool>> DeleteAsync(string endpoint);

        // POST request không cần response data (như logout)
        Task<ApiResponse<bool>> PostAsync(string endpoint, object? data = null);
    }

    // Wrapper class cho API response với metadata
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public HttpStatusCode StatusCode { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, T? data, string message, HttpStatusCode statusCode)
        {
            Success = success;
            Data = data;
            Message = message;
            StatusCode = statusCode;
        }

        // Tạo response thành công
        public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        {
            return new ApiResponse<T>(true, data, message, HttpStatusCode.OK);
        }

        // Tạo response lỗi
        public static ApiResponse<T> ErrorResult(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ApiResponse<T>(false, default(T), message, statusCode);
        }
    }
} 