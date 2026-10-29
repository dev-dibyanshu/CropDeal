using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PaymentService.DTOs;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    [Authorize(Roles = "Dealer")]
    public async Task<IActionResult> ProcessPayment([FromBody] CreatePaymentDto createPaymentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int dealerId))
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        var (success, message, payment) = await _paymentService.ProcessPaymentAsync(dealerId, createPaymentDto);

        if (!success)
        {
            if (message == "Payment already exists for this order")
            {
                return Conflict(new { message });
            }
            return BadRequest(new { message, payment });
        }

        return CreatedAtAction(nameof(GetPaymentById), new { id = payment!.Id }, payment);
    }

    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetPaymentHistory()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (roleClaim == "Dealer")
        {
            var payments = await _paymentService.GetDealerPaymentHistoryAsync(userId);
            return Ok(payments);
        }
        else if (roleClaim == "Farmer")
        {
            var payments = await _paymentService.GetFarmerPaymentHistoryAsync(userId);
            return Ok(payments);
        }
        else if (roleClaim == "Admin")
        {
            // Admin can view all - for simplicity, we'll return dealer history
            // In a real system, you'd have a separate method
            var payments = await _paymentService.GetDealerPaymentHistoryAsync(userId);
            return Ok(payments);
        }

        return Forbid();
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = "Payment not found" });
        }

        return Ok(payment);
    }

    [HttpPut("{id}/refund")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RefundPayment(int id)
    {
        var (success, message, payment) = await _paymentService.RefundPaymentAsync(id);

        if (!success)
        {
            if (message == "Payment not found")
            {
                return NotFound(new { message });
            }
            return BadRequest(new { message });
        }

        return Ok(payment);
    }
}
