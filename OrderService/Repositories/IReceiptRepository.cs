using OrderService.Models;

namespace OrderService.Repositories;

public interface IReceiptRepository
{
    Task<Receipt?> GetByIdAsync(int id);
    Task<Receipt?> GetByOrderIdAsync(int orderId);
    Task<Receipt> CreateAsync(Receipt receipt);
}
