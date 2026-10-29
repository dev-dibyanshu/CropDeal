using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OrderService.DTOs;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize(Roles = "Dealer")]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto createReviewDto)
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

        var (success, message, review) = await _reviewService.CreateReviewAsync(dealerId, createReviewDto);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return CreatedAtAction(nameof(GetFarmerReviews), new { farmerId = review!.FarmerId }, review);
    }

    [HttpGet("farmer/{farmerId}")]
    public async Task<IActionResult> GetFarmerReviews(int farmerId)
    {
        var reviews = await _reviewService.GetFarmerReviewsAsync(farmerId);
        return Ok(reviews);
    }
}
