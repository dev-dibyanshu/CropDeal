using OrderService.DTOs;

namespace OrderService.Services;

public interface IReviewService
{
    Task<(bool Success, string? Message, ReviewResponseDto? Review)> CreateReviewAsync(int dealerId, CreateReviewDto createReviewDto);
    Task<IEnumerable<ReviewResponseDto>> GetFarmerReviewsAsync(int farmerId);
}
