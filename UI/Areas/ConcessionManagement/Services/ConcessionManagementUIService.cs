using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UI.Areas.ConcessionManagement.Models;

namespace UI.Areas.ConcessionManagement.Services
{
    public class ConcessionManagementUIService : IConcessionManagementUIService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ConcessionManagementUIService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<ConcessionItemViewModel>> GetAllConcessionItemsAsync()
        {
            AddAuthToken();
            var response = await _httpClient.GetAsync("/api/ConcessionItems");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ConcessionItemViewModel>>() ?? new List<ConcessionItemViewModel>();
            }
            return new List<ConcessionItemViewModel>();
        }

        public async Task<ConcessionItemViewModel?> GetConcessionItemByIdAsync(Guid id)
        {
            AddAuthToken();
            var response = await _httpClient.GetAsync($"/api/ConcessionItems/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ConcessionItemViewModel>();
            }
            return null;
        }

        public async Task<bool> CreateConcessionItemAsync(ConcessionItemViewModel model)
        {
            AddAuthToken();
            var response = await _httpClient.PostAsJsonAsync("/api/ConcessionItems", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateConcessionItemAsync(Guid id, ConcessionItemViewModel model)
        {
            AddAuthToken();
            var response = await _httpClient.PutAsJsonAsync($"/api/ConcessionItems/{id}", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteConcessionItemAsync(Guid id)
        {
            AddAuthToken();
            var response = await _httpClient.DeleteAsync($"/api/ConcessionItems/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
