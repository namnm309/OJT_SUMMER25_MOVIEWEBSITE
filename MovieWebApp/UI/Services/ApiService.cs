using System.Net;
using System.Text;
using System.Text.Json;

namespace UI.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                _logger.LogInformation("🚀 GET Request: {Endpoint}", endpoint);
                
                var response = await _httpClient.GetAsync(endpoint);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ GET Request failed: {Endpoint}", endpoint);
                return ApiResponse<T>.ErrorResult($"Request failed: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                _logger.LogInformation("POST Request: {Endpoint}", endpoint);
                
                var content = CreateJsonContent(data);
                var response = await _httpClient.PostAsync(endpoint, content);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST Request failed: {Endpoint}", endpoint);
                return ApiResponse<T>.ErrorResult($"Request failed: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> PostAsync(string endpoint, object? data = null)
        {
            try
            {
                _logger.LogInformation("POST Request: {Endpoint}", endpoint);
                
                var content = CreateJsonContent(data);
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    return ApiResponse<bool>.SuccessResult(true, "Request successful");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResponse<bool>.ErrorResult($"Request failed: {response.StatusCode}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST Request failed: {Endpoint}", endpoint);
                return ApiResponse<bool>.ErrorResult($"Request failed: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                _logger.LogInformation("PUT Request: {Endpoint}", endpoint);
                
                var content = CreateJsonContent(data);
                var response = await _httpClient.PutAsync(endpoint, content);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PUT Request failed: {Endpoint}", endpoint);
                return ApiResponse<T>.ErrorResult($"Request failed: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(string endpoint)
        {
            try
            {
                _logger.LogInformation("DELETE Request: {Endpoint}", endpoint);
                
                var response = await _httpClient.DeleteAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    return ApiResponse<bool>.SuccessResult(true, "Delete successful");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResponse<bool>.ErrorResult($"Delete failed: {response.StatusCode}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " DELETE Request failed: {Endpoint}", endpoint);
                return ApiResponse<bool>.ErrorResult($"Request failed: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }

        private StringContent? CreateJsonContent(object? data)
        {
            if (data == null) return null;
            
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private async Task<ApiResponse<T>> ProcessResponse<T>(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("📥 Response Status: {StatusCode}, Content: {Content}", 
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // Parse response as JsonElement để check structure trước
                    var apiResponseElement = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    // Kiểm tra API response format { success, data/user, message }
                    if (apiResponseElement.TryGetProperty("success", out var successProp))
                    {
                        var success = successProp.GetBoolean();
                        var message = apiResponseElement.TryGetProperty("message", out var msgProp) 
                            ? msgProp.GetString() ?? "Success" 
                            : "Success";
                            
                        if (success && apiResponseElement.TryGetProperty("data", out var dataProp))
                        {
                            // Response có "data" property
                            if (typeof(T) == typeof(JsonElement))
                            {
                                return ApiResponse<T>.SuccessResult((T)(object)dataProp, message);
                            }
                            var data = JsonSerializer.Deserialize<T>(dataProp.GetRawText(), _jsonOptions);
                            return ApiResponse<T>.SuccessResult(data!, message);
                        }
                        else if (success && apiResponseElement.TryGetProperty("user", out var userProp))
                        {
                            // Special case for login response có "user" property
                            if (typeof(T) == typeof(JsonElement))
                            {
                                return ApiResponse<T>.SuccessResult((T)(object)userProp, message);
                            }
                            var userData = JsonSerializer.Deserialize<T>(userProp.GetRawText(), _jsonOptions);
                            return ApiResponse<T>.SuccessResult(userData!, message);
                        }
                        else
                        {
                            return ApiResponse<T>.ErrorResult(message, response.StatusCode);
                        }
                    }
                    
                    // Fallback: không có API format, parse trực tiếp
                    if (typeof(T) == typeof(JsonElement) || typeof(T) == typeof(object))
                    {
                        return ApiResponse<T>.SuccessResult((T)(object)apiResponseElement, "Success");
                    }
                    
                    var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                    return ApiResponse<T>.SuccessResult(result!, "Success");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON Deserialization failed for response: {Content}", responseContent);
                    return ApiResponse<T>.ErrorResult("Failed to parse response", response.StatusCode);
                }
            }
            else
            {
                // Handle error response
                try
                {
                    var errorElement = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var message = errorElement.TryGetProperty("message", out var msgProp) 
                        ? msgProp.GetString() ?? "Request failed" 
                        : "Request failed";
                        
                    return ApiResponse<T>.ErrorResult(message, response.StatusCode);
                }
                catch
                {
                    return ApiResponse<T>.ErrorResult($"Request failed: {response.StatusCode}", response.StatusCode);
                }
            }
        }
    }
} 