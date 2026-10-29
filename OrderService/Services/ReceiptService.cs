using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;

namespace OrderService.Services;

public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IOrderRepository _orderRepository;

    public ReceiptService(IReceiptRepository receiptRepository, IOrderRepository orderRepository)
    {
        _receiptRepository = receiptRepository;
        _orderRepository = orderRepository;
    }

    public async Task<ReceiptResponseDto?> GetReceiptByIdAsync(int id)
    {
        var receipt = await _receiptRepository.GetByIdAsync(id);
        if (receipt == null)
        {
            return null;
        }

        return MapToResponseDto(receipt);
    }

    public async Task<(bool Success, string? Message, ReceiptResponseDto? Receipt)> GenerateReceiptAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return (false, "Order not found", null);
        }

        if (order.Status != OrderStatus.Completed)
        {
            return (false, "Receipt can only be generated for completed orders", null);
        }

        var existingReceipt = await _receiptRepository.GetByOrderIdAsync(orderId);
        if (existingReceipt != null)
        {
            return (false, "Receipt already exists for this order", null);
        }

        var receipt = new Receipt
        {
            OrderId = orderId,
            ReceiptNumber = $"REC-{orderId:D6}-{DateTime.UtcNow:yyyyMMdd}",
            ReceiptDate = DateTime.UtcNow,
            DocumentUrl = $"/receipts/{orderId}.pdf"
        };

        var createdReceipt = await _receiptRepository.CreateAsync(receipt);

        return (true, "Receipt generated successfully", MapToResponseDto(createdReceipt));
    }

    private ReceiptResponseDto MapToResponseDto(Receipt receipt)
    {
        return new ReceiptResponseDto
        {
            Id = receipt.Id,
            OrderId = receipt.OrderId,
            ReceiptNumber = receipt.ReceiptNumber,
            ReceiptDate = receipt.ReceiptDate,
            DocumentUrl = receipt.DocumentUrl
        };
    }
}
