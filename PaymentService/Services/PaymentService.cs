using PaymentService.DTOs;
using PaymentService.Models;
using PaymentService.Repositories;

namespace PaymentService.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<(bool Success, string? Message, PaymentResponseDto? Payment)> ProcessPaymentAsync(int dealerId, CreatePaymentDto createPaymentDto)
    {
        // Check if payment already exists for this order
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(createPaymentDto.OrderId);
        if (existingPayment != null)
        {
            return (false, "Payment already exists for this order", null);
        }

        // Generate transaction reference
        var transactionReference = GenerateTransactionReference();

        // Create payment record
        var payment = new Payment
        {
            OrderId = createPaymentDto.OrderId,
            DealerId = dealerId,
            FarmerId = createPaymentDto.FarmerId,
            Amount = createPaymentDto.Amount,
            PaymentMethod = createPaymentDto.PaymentMethod,
            PaymentStatus = PaymentStatus.Pending,
            TransactionReference = transactionReference,
            CreatedAt = DateTime.UtcNow
        };

        // Simulate payment processing (fake implementation)
        // In a real system, this would call a payment gateway
        var paymentSuccess = SimulatePaymentGateway(payment);

        if (paymentSuccess)
        {
            payment.PaymentStatus = PaymentStatus.Successful;
        }
        else
        {
            payment.PaymentStatus = PaymentStatus.Failed;
        }

        var createdPayment = await _paymentRepository.CreateAsync(payment);

        if (payment.PaymentStatus == PaymentStatus.Successful)
        {
            return (true, "Payment processed successfully", MapToResponseDto(createdPayment));
        }
        else
        {
            return (false, "Payment processing failed", MapToResponseDto(createdPayment));
        }
    }

    public async Task<IEnumerable<PaymentHistoryDto>> GetDealerPaymentHistoryAsync(int dealerId)
    {
        var payments = await _paymentRepository.GetByDealerIdAsync(dealerId);
        return payments.Select(MapToHistoryDto);
    }

    public async Task<IEnumerable<PaymentHistoryDto>> GetFarmerPaymentHistoryAsync(int farmerId)
    {
        var payments = await _paymentRepository.GetByFarmerIdAsync(farmerId);
        return payments.Select(MapToHistoryDto);
    }

    public async Task<PaymentResponseDto?> GetPaymentByIdAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment == null)
        {
            return null;
        }

        return MapToResponseDto(payment);
    }

    public async Task<(bool Success, string? Message, PaymentResponseDto? Payment)> RefundPaymentAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
        {
            return (false, "Payment not found", null);
        }

        if (payment.PaymentStatus != PaymentStatus.Successful)
        {
            return (false, "Only successful payments can be refunded", null);
        }

        if (payment.PaymentStatus == PaymentStatus.Refunded)
        {
            return (false, "Payment is already refunded", null);
        }

        // Update payment status to Refunded
        payment.PaymentStatus = PaymentStatus.Refunded;
        var updatedPayment = await _paymentRepository.UpdateAsync(payment);

        return (true, "Payment refunded successfully", MapToResponseDto(updatedPayment));
    }

    private string GenerateTransactionReference()
    {
        // Generate a simple transaction reference
        // Format: TXN-{timestamp}-{random}
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        return $"TXN-{timestamp}-{random}";
    }

    private bool SimulatePaymentGateway(Payment payment)
    {
        // Fake payment gateway simulation
        // In a real system, this would integrate with Stripe, PayPal, Razorpay, etc.
        // For this evaluation project, we simulate a successful payment
        
        // Simulate 95% success rate (just for demonstration)
        // In practice, you'd want 100% success for testing
        return true; // Always succeed for testing
    }

    private PaymentResponseDto MapToResponseDto(Payment payment)
    {
        return new PaymentResponseDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            DealerId = payment.DealerId,
            FarmerId = payment.FarmerId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
            TransactionReference = payment.TransactionReference,
            CreatedAt = payment.CreatedAt
        };
    }

    private PaymentHistoryDto MapToHistoryDto(Payment payment)
    {
        return new PaymentHistoryDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            PaymentStatus = payment.PaymentStatus,
            TransactionReference = payment.TransactionReference,
            CreatedAt = payment.CreatedAt
        };
    }
}
