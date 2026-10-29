using OrderService.Models;

namespace OrderService.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id);
    Task<IEnumerable<Review>> GetByFarmerIdAsync(int farmerId);
    Task<Review?> GetByOrderIdAsync(int orderId);
    Task<Review> CreateAsync(Review review);
}
