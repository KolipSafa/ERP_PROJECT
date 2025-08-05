using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Supabase.Gotrue; // User nesnesi için

namespace Infrastructure.Services
{
    public class SupabaseAuthAdminService : ISupabaseAuthAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SupabaseAuthAdminService> _logger;

        public SupabaseAuthAdminService(HttpClient httpClient, IConfiguration configuration, ILogger<SupabaseAuthAdminService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            var supabaseUrl = configuration["Supabase:Url"] ?? throw new ArgumentException("Supabase:Url is required");
            var serviceRoleKey = configuration["Supabase:ServiceRoleKey"] ?? throw new ArgumentException("Supabase:ServiceRoleKey is required");
            
            _httpClient.BaseAddress = new Uri($"{supabaseUrl}/auth/v1/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {serviceRoleKey}");
            _httpClient.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
        }

        public async Task DeleteUser(string userId, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.DeleteAsync($"admin/users/{userId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to delete user {UserId}. Status: {StatusCode}, Response: {Response}", userId, response.StatusCode, errorContent);
            }
        }

        public async Task<User?> UpdateUserAppMetadata(string userId, Dictionary<string, object> metadata)
        {
            var requestBody = new { app_metadata = metadata };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"admin/users/{userId}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var updatedUser = JsonSerializer.Deserialize<User>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _logger.LogInformation("Successfully updated app_metadata for user {UserId}.", userId);
                return updatedUser;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to update app_metadata for user {UserId}. Status: {StatusCode}, Response: {Response}", userId, response.StatusCode, errorContent);
                throw new HttpRequestException($"Failed to update user metadata. Status: {response.StatusCode}, Response: {errorContent}");
            }
        }
    }
}
