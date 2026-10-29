using OrderService.Models;

namespace OrderService.Repositories;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(int id);
    Task<Invoice?> GetByOrderIdAsync(int orderId);
    Task<Invoice> CreateAsync(Invoice invoice);
}
