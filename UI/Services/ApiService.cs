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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
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
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                AddAuthenticationHeaders(request);
                
                var response = await _httpClient.SendAsync(request);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET Request failed: {Endpoint}", endpoint);
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
                string? backendMsg = null;
                try {
                    var errJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errJson.TryGetProperty("message", out var msgProp)) backendMsg = msgProp.GetString();
                    else if (errJson.TryGetProperty("Message", out var msgProp2)) backendMsg = msgProp2.GetString();
                } catch { }

                var finalMsg = backendMsg ?? $"Request failed: {response.StatusCode}";
                return ApiResponse<bool>.ErrorResult(finalMsg, response.StatusCode);
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

        public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                _logger.LogInformation("PATCH Request: {Endpoint}", endpoint);
                
                var content = CreateJsonContent(data);
                var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
                {
                    Content = content
                };
                
                var response = await _httpClient.SendAsync(request);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PATCH Request failed: {Endpoint}", endpoint);
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

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        _logger.LogWarning("Empty response content received");
                        return ApiResponse<T>.ErrorResult("Empty response received", response.StatusCode);
                    }
                    
                    // Trường hợp đặc biệt nếu T là JsonElement, trả về toàn bộ response
                    if (typeof(T) == typeof(JsonElement))
                    {
                        var element = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        return ApiResponse<T>.SuccessResult((T)(object)element, "Success");
                    }
                    

                    var apiResponseElement = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    // Kiểm tra API response format { success, data/user, message } (UI format)
                    if (apiResponseElement.TryGetProperty("success", out var successProp))
                    {
                        var success = successProp.GetBoolean();
                        var message = apiResponseElement.TryGetProperty("message", out var msgProp) 
                            ? msgProp.GetString() ?? "Success" 
                            : "Success";
                            
                        if (success)
                        {
                            // Kiểm tra xem có property data không (lowercase - UI format)
                            if (apiResponseElement.TryGetProperty("data", out var dataProp))
                            {
                                try
                                {
                                    var data = JsonSerializer.Deserialize<T>(dataProp.GetRawText(), _jsonOptions);
                                    return ApiResponse<T>.SuccessResult(data!, message);
                                }
                                catch (JsonException ex)
                                {
                                    _logger.LogError(ex, "Failed to deserialize 'data' property: {Content}", dataProp.GetRawText());
                                    return ApiResponse<T>.ErrorResult($"Failed to parse data: {ex.Message}", response.StatusCode);
                                }
                            }
                            // Kiểm tra xem có property Data không (capital D - Backend format)
                            else if (apiResponseElement.TryGetProperty("Data", out var dataPropCapital))
                            {
                                try
                                {
                                    var data = JsonSerializer.Deserialize<T>(dataPropCapital.GetRawText(), _jsonOptions);
                                    return ApiResponse<T>.SuccessResult(data!, message);
                                }
                                catch (JsonException ex)
                                {
                                    _logger.LogError(ex, "Failed to deserialize 'Data' property: {Content}", dataPropCapital.GetRawText());
                                    return ApiResponse<T>.ErrorResult($"Failed to parse Data: {ex.Message}", response.StatusCode);
                                }
                            }
                            // Kiểm tra xem có property user không (đặc biệt cho login)
                            else if (apiResponseElement.TryGetProperty("user", out var userProp))
                            {
                                try
                                {
                                    var userData = JsonSerializer.Deserialize<T>(userProp.GetRawText(), _jsonOptions);
                                    return ApiResponse<T>.SuccessResult(userData!, message);
                                }
                                catch (JsonException ex)
                                {
                                    _logger.LogError(ex, "Failed to deserialize 'user' property: {Content}", userProp.GetRawText());
                                    return ApiResponse<T>.ErrorResult($"Failed to parse user data: {ex.Message}", response.StatusCode);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Success response without data or user property");
                                
                                // Nếu T là kiểu bool hoặc object, có thể trả về success mà không cần data
                                if (typeof(T) == typeof(bool) || typeof(T) == typeof(object))
                                {
                                    return ApiResponse<T>.SuccessResult((T)(object)(success), message);
                                }
                                
                                return ApiResponse<T>.ErrorResult($"Success response without data: {message}", response.StatusCode);
                            }
                        }
                        else
                        {
                            return ApiResponse<T>.ErrorResult(message, response.StatusCode);
                        }
                    }
                    // Kiểm tra API response format { code, data, message } (Backend format)
                    else if (apiResponseElement.TryGetProperty("code", out var codeProp))
                    {
                        var code = codeProp.GetInt32();
                        var message = apiResponseElement.TryGetProperty("message", out var msgProp) 
                            ? msgProp.GetString() ?? "Success" 
                            : "Success";
                            
                        if (code >= 200 && code < 300) // Success codes
                        {
                            // Kiểm tra xem có property data không (lowercase - UI format)
                            if (apiResponseElement.TryGetProperty("data", out var dataProp))
                            {
                                try
                                {
                                    var data = JsonSerializer.Deserialize<T>(dataProp.GetRawText(), _jsonOptions);
                                    return ApiResponse<T>.SuccessResult(data!, message);
                                }
                                catch (JsonException ex)
                                {
                                    _logger.LogError(ex, "Failed to deserialize 'data' property: {Content}", dataProp.GetRawText());
                                    return ApiResponse<T>.ErrorResult($"Failed to parse data: {ex.Message}", response.StatusCode);
                                }
                            }
                            // Kiểm tra xem có property Data không (capital D - Backend format)
                            else if (apiResponseElement.TryGetProperty("Data", out var dataPropCapital))
                            {
                                try
                                {
                                    var data = JsonSerializer.Deserialize<T>(dataPropCapital.GetRawText(), _jsonOptions);
                                    return ApiResponse<T>.SuccessResult(data!, message);
                                }
                                catch (JsonException ex)
                                {
                                    _logger.LogError(ex, "Failed to deserialize 'Data' property: {Content}", dataPropCapital.GetRawText());
                                    return ApiResponse<T>.ErrorResult($"Failed to parse Data: {ex.Message}", response.StatusCode);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Success response without data property");
                                
                                // Nếu T là kiểu bool hoặc object, có thể trả về success mà không cần data
                                if (typeof(T) == typeof(bool) || typeof(T) == typeof(object))
                                {
                                    return ApiResponse<T>.SuccessResult((T)(object)(true), message);
                                }
                                
                                return ApiResponse<T>.ErrorResult($"Success response without data: {message}", response.StatusCode);
                            }
                        }
                        else
                        {
                            return ApiResponse<T>.ErrorResult(message, response.StatusCode);
                        }
                    }
                    
                    // Fallback: không có API format, parse trực tiếp (cho Backend API responses)
                    try
                    {
                        // Nếu T là dynamic, trả về JsonElement
                        if (typeof(T) == typeof(object) || typeof(T).Name == "Object")
                        {
                            return ApiResponse<T>.SuccessResult((T)(object)apiResponseElement, "Success");
                        }
                        
                        var result = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                        
                        // Kiểm tra null safety
                        if (result != null)
                        {
                            return ApiResponse<T>.SuccessResult(result, "Success");
                        }
                        else
                        {
                            _logger.LogWarning("Deserialization returned null result");
                            return ApiResponse<T>.ErrorResult("Response data is null", response.StatusCode);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Direct deserialization failed: {Content}", responseContent);
                        return ApiResponse<T>.ErrorResult($"Failed to parse response directly: {ex.Message}", response.StatusCode);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "JSON Deserialization failed for response: {Content}", responseContent);
                    return ApiResponse<T>.ErrorResult($"Failed to parse response: {ex.Message}", response.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error processing response: {Content}", responseContent);
                    return ApiResponse<T>.ErrorResult($"Unexpected error: {ex.Message}", response.StatusCode);
                }
            }
            else
            {

                try
                {
                    if (string.IsNullOrWhiteSpace(responseContent))
                    {
                        return ApiResponse<T>.ErrorResult($"Request failed with status: {response.StatusCode}", response.StatusCode);
                    }
                    
                    var errorElement = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    string message;
                    if (errorElement.TryGetProperty("message", out var msgProp))
                        message = msgProp.GetString() ?? $"Request failed with status: {response.StatusCode}";
                    else if (errorElement.TryGetProperty("Message", out var msgProp2))
                        message = msgProp2.GetString() ?? $"Request failed with status: {response.StatusCode}";
                    else
                        message = $"Request failed with status: {response.StatusCode}";
                        
                    return ApiResponse<T>.ErrorResult(message, response.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse error response: {Content}", responseContent);
                    return ApiResponse<T>.ErrorResult($"Request failed: {response.StatusCode}", response.StatusCode);
                }
            }
        }

        private void AddAuthenticationHeaders(HttpRequestMessage request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.Cookies != null)
            {
                var cookieHeader = string.Join("; ", httpContext.Request.Cookies.Select(c => $"{c.Key}={c.Value}"));
                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    request.Headers.Add("Cookie", cookieHeader);
                }
            }
        }
    }
}