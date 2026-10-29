using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;

namespace OrderService.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IOrderRepository _orderRepository;

    public ReviewService(IReviewRepository reviewRepository, IOrderRepository orderRepository)
    {
        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
    }

    public async Task<(bool Success, string? Message, ReviewResponseDto? Review)> CreateReviewAsync(int dealerId, CreateReviewDto createReviewDto)
    {
        var order = await _orderRepository.GetByIdAsync(createReviewDto.OrderId);
        if (order == null)
        {
            return (false, "Order not found", null);
        }

        if (order.DealerId != dealerId)
        {
            return (false, "Unauthorized to review this order", null);
        }

        if (order.Status != OrderStatus.Completed)
        {
            return (false, "Reviews can only be submitted for completed orders", null);
        }

        var existingReview = await _reviewRepository.GetByOrderIdAsync(createReviewDto.OrderId);
        if (existingReview != null)
        {
            return (false, "Review already exists for this order", null);
        }

        var review = new Review
        {
            OrderId = createReviewDto.OrderId,
            DealerId = dealerId,
            FarmerId = createReviewDto.FarmerId,
            Rating = createReviewDto.Rating,
            Comment = createReviewDto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        var createdReview = await _reviewRepository.CreateAsync(review);

        return (true, "Review submitted successfully", MapToResponseDto(createdReview));
    }

    public async Task<IEnumerable<ReviewResponseDto>> GetFarmerReviewsAsync(int farmerId)
    {
        var reviews = await _reviewRepository.GetByFarmerIdAsync(farmerId);
        return reviews.Select(MapToResponseDto);
    }

    private ReviewResponseDto MapToResponseDto(Review review)
    {
        return new ReviewResponseDto
        {
            Id = review.Id,
            OrderId = review.OrderId,
            DealerId = review.DealerId,
            FarmerId = review.FarmerId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }
}
