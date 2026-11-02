using System.Text;
using System.Text.Json;
using OrderService.DTOs;

namespace OrderService.Services;

public class CropServiceClient : ICropServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CropServiceClient> _logger;

    public CropServiceClient(HttpClient httpClient, ILogger<CropServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CropListingDto?> GetCropListingAsync(int listingId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/crops/listings/{listingId}");
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get crop listing {ListingId}. Status: {StatusCode}", 
                    listingId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            return JsonSerializer.Deserialize<CropListingDto>(content, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting crop listing {ListingId}", listingId);
            return null;
        }
    }

    public async Task<bool> UpdateListingQuantityAsync(int listingId, decimal newAvailableQuantity)
    {
        try
        {
            var updateDto = new { AvailableQuantity = newAvailableQuantity };
            var json = JsonSerializer.Serialize(updateDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/crops/listings/{listingId}/quantity", content);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating crop listing quantity {ListingId}", listingId);
            return false;
        }
    }
}
