using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CropService.DTOs;
using CropService.Services;

namespace CropService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CropController : ControllerBase
{
    private readonly ICropService _cropService;

    public CropController(ICropService cropService)
    {
        _cropService = cropService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCrops()
    {
        var crops = await _cropService.GetAllCropsAsync();
        return Ok(crops);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCropById(int id)
    {
        var crop = await _cropService.GetCropByIdAsync(id);
        if (crop == null)
        {
            return NotFound(new { message = "Crop not found" });
        }

        return Ok(crop);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCrop([FromBody] CreateCropDto createCropDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var crop = await _cropService.CreateCropAsync(createCropDto);
        return CreatedAtAction(nameof(GetCropById), new { id = crop.Id }, crop);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCrop(int id, [FromBody] UpdateCropDto updateCropDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var crop = await _cropService.UpdateCropAsync(id, updateCropDto);
        if (crop == null)
        {
            return NotFound(new { message = "Crop not found" });
        }

        return Ok(crop);
    }
}
