using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly AppDbContext _context;

    public ReceiptRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Receipt?> GetByIdAsync(int id)
    {
        return await _context.Receipts
            .Include(r => r.Order)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Receipt?> GetByOrderIdAsync(int orderId)
    {
        return await _context.Receipts
            .FirstOrDefaultAsync(r => r.OrderId == orderId);
    }

    public async Task<Receipt> CreateAsync(Receipt receipt)
    {
        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();
        return receipt;
    }
}
