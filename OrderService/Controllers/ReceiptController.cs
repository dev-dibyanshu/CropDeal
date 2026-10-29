using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptController : ControllerBase
{
    private readonly IReceiptService _receiptService;

    public ReceiptController(IReceiptService receiptService)
    {
        _receiptService = receiptService;
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetReceiptById(int id)
    {
        var receipt = await _receiptService.GetReceiptByIdAsync(id);
        if (receipt == null)
        {
            return NotFound(new { message = "Receipt not found" });
        }

        return Ok(receipt);
    }

    [HttpPost("generate/{orderId}")]
    [Authorize(Roles = "Admin,Farmer")]
    public async Task<IActionResult> GenerateReceipt(int orderId)
    {
        var (success, message, receipt) = await _receiptService.GenerateReceiptAsync(orderId);

        if (!success)
        {
            if (message == "Order not found")
            {
                return NotFound(new { message });
            }
            return BadRequest(new { message });
        }

        return CreatedAtAction(nameof(GetReceiptById), new { id = receipt!.Id }, receipt);
    }
}
