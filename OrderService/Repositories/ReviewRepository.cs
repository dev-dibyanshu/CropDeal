using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _context.Reviews.FindAsync(id);
    }

    public async Task<IEnumerable<Review>> GetByFarmerIdAsync(int farmerId)
    {
        return await _context.Reviews
            .Where(r => r.FarmerId == farmerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Review?> GetByOrderIdAsync(int orderId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.OrderId == orderId);
    }

    public async Task<Review> CreateAsync(Review review)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }
}
