using PaymentService.Models;

namespace PaymentService.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id);
    Task<IEnumerable<Payment>> GetByDealerIdAsync(int dealerId);
    Task<IEnumerable<Payment>> GetByFarmerIdAsync(int farmerId);
    Task<Payment?> GetByOrderIdAsync(int orderId);
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment> UpdateAsync(Payment payment);
}
