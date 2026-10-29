using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OrderService.DTOs;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [Authorize(Roles = "Dealer")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
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

        var (success, message, order) = await _orderService.CreateOrderAsync(dealerId, createOrderDto);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return CreatedAtAction(nameof(GetOrderById), new { id = order!.Id }, order);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        return Ok(order);
    }

    [HttpGet("my-orders")]
    [Authorize(Roles = "Dealer")]
    public async Task<IActionResult> GetMyOrders()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int dealerId))
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        var orders = await _orderService.GetDealerOrdersAsync(dealerId);
        return Ok(orders);
    }

    [HttpGet("received")]
    [Authorize(Roles = "Farmer")]
    public async Task<IActionResult> GetReceivedOrders()
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int farmerId))
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        var orders = await _orderService.GetFarmerOrdersAsync(farmerId);
        return Ok(orders);
    }

    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Dealer")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int dealerId))
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        var (success, message, order) = await _orderService.CancelOrderAsync(id, dealerId);

        if (!success)
        {
            if (message == "Order not found")
            {
                return NotFound(new { message });
            }
            if (message == "Unauthorized to cancel this order")
            {
                return StatusCode(403, new { message });
            }
            return BadRequest(new { message });
        }

        return Ok(order);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Farmer")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateStatusDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message, order) = await _orderService.UpdateOrderStatusAsync(id, updateStatusDto);

        if (!success)
        {
            if (message == "Order not found")
            {
                return NotFound(new { message });
            }
            return BadRequest(new { message });
        }

        return Ok(order);
    }
}
