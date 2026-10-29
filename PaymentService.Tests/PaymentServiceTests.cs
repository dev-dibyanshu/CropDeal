using Moq;
using PaymentService.Services;
using PaymentService.Repositories;
using PaymentService.DTOs;
using PaymentService.Models;

namespace PaymentService.Tests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly IPaymentService _paymentService;

    public PaymentServiceTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _paymentService = new Services.PaymentService(_mockPaymentRepository.Object);
    }

    [Fact]
    public async Task ProcessPayment_WithValidData_ShouldCreatePayment()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreatePaymentDto
        {
            OrderId = 1,
            FarmerId = 2,
            Amount = 5000,
            PaymentMethod = "CreditCard"
        };

        _mockPaymentRepository
            .Setup(x => x.GetByOrderIdAsync(createDto.OrderId))
            .ReturnsAsync((Payment?)null);

        _mockPaymentRepository
            .Setup(x => x.CreateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // Act
        var result = await _paymentService.ProcessPaymentAsync(dealerId, createDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Payment processed successfully", result.Message);
        Assert.NotNull(result.Payment);
        Assert.Equal(PaymentStatus.Successful, result.Payment.PaymentStatus);
        Assert.NotNull(result.Payment.TransactionReference);
        _mockPaymentRepository.Verify(x => x.CreateAsync(It.Is<Payment>(p =>
            p.OrderId == createDto.OrderId &&
            p.DealerId == dealerId &&
            p.Amount == createDto.Amount)), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_WhenPaymentAlreadyExists_ShouldFail()
    {
        // Arrange
        var dealerId = 1;
        var createDto = new CreatePaymentDto
        {
            OrderId = 1,
            FarmerId = 2,
            Amount = 5000,
            PaymentMethod = "CreditCard"
        };

        var existingPayment = new Payment
        {
            Id = 1,
            OrderId = createDto.OrderId,
            PaymentStatus = PaymentStatus.Successful
        };

        _mockPaymentRepository
            .Setup(x => x.GetByOrderIdAsync(createDto.OrderId))
            .ReturnsAsync(existingPayment);

        // Act
        var result = await _paymentService.ProcessPaymentAsync(dealerId, createDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Payment already exists for this order", result.Message);
        Assert.Null(result.Payment);
        _mockPaymentRepository.Verify(x => x.CreateAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task GetPaymentById_WithValidId_ShouldReturnPayment()
    {
        // Arrange
        var payment = new Payment
        {
            Id = 1,
            OrderId = 1,
            DealerId = 1,
            FarmerId = 2,
            Amount = 5000,
            PaymentStatus = PaymentStatus.Successful,
            TransactionReference = "TXN-12345"
        };

        _mockPaymentRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(payment);

        // Act
        var result = await _paymentService.GetPaymentByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(payment.Id, result.Id);
        Assert.Equal(payment.Amount, result.Amount);
    }

    [Fact]
    public async Task GetPaymentById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockPaymentRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _paymentService.GetPaymentByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RefundPayment_WithValidPayment_ShouldRefund()
    {
        // Arrange
        var paymentId = 1;
        var payment = new Payment
        {
            Id = paymentId,
            OrderId = 1,
            Amount = 5000,
            PaymentStatus = PaymentStatus.Successful
        };

        _mockPaymentRepository
            .Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        _mockPaymentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => p);

        // Act
        var result = await _paymentService.RefundPaymentAsync(paymentId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Payment refunded successfully", result.Message);
        Assert.NotNull(result.Payment);
        Assert.Equal(PaymentStatus.Refunded, result.Payment.PaymentStatus);
        _mockPaymentRepository.Verify(x => x.UpdateAsync(It.IsAny<Payment>()), Times.Once);
    }

    [Fact]
    public async Task RefundPayment_WithNonExistentPayment_ShouldFail()
    {
        // Arrange
        var paymentId = 999;

        _mockPaymentRepository
            .Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _paymentService.RefundPaymentAsync(paymentId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Payment not found", result.Message);
        Assert.Null(result.Payment);
    }

    [Fact]
    public async Task RefundPayment_WithFailedPayment_ShouldFail()
    {
        // Arrange
        var paymentId = 1;
        var payment = new Payment
        {
            Id = paymentId,
            PaymentStatus = PaymentStatus.Failed
        };

        _mockPaymentRepository
            .Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        // Act
        var result = await _paymentService.RefundPaymentAsync(paymentId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Only successful payments can be refunded", result.Message);
        _mockPaymentRepository.Verify(x => x.UpdateAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task RefundPayment_AlreadyRefunded_ShouldFail()
    {
        // Arrange
        var paymentId = 1;
        var payment = new Payment
        {
            Id = paymentId,
            PaymentStatus = PaymentStatus.Refunded
        };

        _mockPaymentRepository
            .Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        // Act
        var result = await _paymentService.RefundPaymentAsync(paymentId);

        // Assert
        Assert.False(result.Success);
        // The implementation checks PaymentStatus != Successful first, which catches Refunded status
        Assert.Equal("Only successful payments can be refunded", result.Message);
    }

    [Fact]
    public async Task GetDealerPaymentHistory_ShouldReturnPayments()
    {
        // Arrange
        var dealerId = 1;
        var payments = new List<Payment>
        {
            new Payment { Id = 1, DealerId = dealerId, Amount = 1000 },
            new Payment { Id = 2, DealerId = dealerId, Amount = 2000 }
        };

        _mockPaymentRepository
            .Setup(x => x.GetByDealerIdAsync(dealerId))
            .ReturnsAsync(payments);

        // Act
        var result = await _paymentService.GetDealerPaymentHistoryAsync(dealerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetFarmerPaymentHistory_ShouldReturnPayments()
    {
        // Arrange
        var farmerId = 2;
        var payments = new List<Payment>
        {
            new Payment { Id = 1, FarmerId = farmerId, Amount = 1000 }
        };

        _mockPaymentRepository
            .Setup(x => x.GetByFarmerIdAsync(farmerId))
            .ReturnsAsync(payments);

        // Act
        var result = await _paymentService.GetFarmerPaymentHistoryAsync(farmerId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }
}
