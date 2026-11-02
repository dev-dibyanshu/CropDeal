using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CropService.DTOs;
using CropService.Services;

namespace CropService.Controllers;

[ApiController]
[Route("api/crops/listings")]
public class CropListingController : ControllerBase
{
    private readonly ICropListingService _listingService;

    public CropListingController(ICropListingService listingService)
    {
        _listingService = listingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllListings([FromQuery] string? category = null, [FromQuery] string? cropName = null, [FromQuery] string? location = null)
    {
        var listings = await _listingService.GetAllListingsAsync(category, cropName, location);
        return Ok(listings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetListingById(int id)
    {
        var listing = await _listingService.GetListingByIdAsync(id);
        if (listing == null)
        {
            return NotFound(new { message = "Listing not found" });
        }

        return Ok(listing);
    }

    [HttpPost]
    [Authorize(Roles = "Farmer")]
    public async Task<IActionResult> CreateListing([FromBody] CreateCropListingDto createListingDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int farmerId))
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        var (success, message, listing) = await _listingService.CreateListingAsync(farmerId, createListingDto);

        if (!success)
        {
            return BadRequest(new { message });
        }

        return CreatedAtAction(nameof(GetListingById), new { id = listing!.Id }, listing);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Farmer")]
    public async Task<IActionResult> UpdateListing(int id, [FromBody] UpdateCropListingDto updateListingDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int farmerId))
        {
            return Unauthorized(new { message = "Invalid user authentication" });
        }

        var (success, message, listing) = await _listingService.UpdateListingAsync(id, farmerId, updateListingDto);

        if (!success)
        {
            if (message == "Listing not found")
            {
                return NotFound(new { message });
            }
            if (message == "Unauthorized to update this listing")
            {
                return StatusCode(403, new { message });
            }
            return BadRequest(new { message });
        }

        return Ok(listing);
    }

    [HttpPut("{id}/quantity")]
    public async Task<IActionResult> UpdateAvailableQuantity(int id, [FromBody] UpdateQuantityDto updateQuantityDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (success, message) = await _listingService.UpdateAvailableQuantityAsync(id, updateQuantityDto.AvailableQuantity);

        if (!success)
        {
            if (message == "Listing not found")
            {
                return NotFound(new { message });
            }
            return BadRequest(new { message });
        }

        return Ok(new { message });
    }
}
