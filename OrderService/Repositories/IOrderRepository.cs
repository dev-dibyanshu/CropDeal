using OrderService.Models;

namespace OrderService.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetByDealerIdAsync(int dealerId);
    Task<IEnumerable<Order>> GetByFarmerIdAsync(int farmerId);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
}
