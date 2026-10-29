using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoiceController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return NotFound(new { message = "Invoice not found" });
        }

        return Ok(invoice);
    }

    [HttpPost("generate/{orderId}")]
    [Authorize(Roles = "Admin,Farmer")]
    public async Task<IActionResult> GenerateInvoice(int orderId)
    {
        var (success, message, invoice) = await _invoiceService.GenerateInvoiceAsync(orderId);

        if (!success)
        {
            if (message == "Order not found")
            {
                return NotFound(new { message });
            }
            return BadRequest(new { message });
        }

        return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice!.Id }, invoice);
    }
}
