using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;

namespace OrderService.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IOrderRepository _orderRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository, IOrderRepository orderRepository)
    {
        _invoiceRepository = invoiceRepository;
        _orderRepository = orderRepository;
    }

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(int id)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id);
        if (invoice == null)
        {
            return null;
        }

        return MapToResponseDto(invoice);
    }

    public async Task<(bool Success, string? Message, InvoiceResponseDto? Invoice)> GenerateInvoiceAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            return (false, "Order not found", null);
        }

        if (order.Status != OrderStatus.Completed)
        {
            return (false, "Invoice can only be generated for completed orders", null);
        }

        var existingInvoice = await _invoiceRepository.GetByOrderIdAsync(orderId);
        if (existingInvoice != null)
        {
            return (false, "Invoice already exists for this order", null);
        }

        var invoice = new Invoice
        {
            OrderId = orderId,
            InvoiceNumber = $"INV-{orderId:D6}-{DateTime.UtcNow:yyyyMMdd}",
            InvoiceDate = DateTime.UtcNow,
            TotalAmount = order.TotalAmount
        };

        var createdInvoice = await _invoiceRepository.CreateAsync(invoice);

        return (true, "Invoice generated successfully", MapToResponseDto(createdInvoice));
    }

    private InvoiceResponseDto MapToResponseDto(Invoice invoice)
    {
        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            OrderId = invoice.OrderId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            TotalAmount = invoice.TotalAmount
        };
    }
}
