using Microsoft.EntityFrameworkCore;
using CropService.Data;
using CropService.Models;

namespace CropService.Repositories;

public class CropListingRepository : ICropListingRepository
{
    private readonly AppDbContext _context;

    public CropListingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CropListing>> GetAllAsync(string? category = null, string? cropName = null, string? location = null)
    {
        var query = _context.CropListings
            .Include(cl => cl.Crop)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(cl => cl.Crop.Category.Contains(category));
        }

        if (!string.IsNullOrWhiteSpace(cropName))
        {
            query = query.Where(cl => cl.Crop.CropName.Contains(cropName));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(cl => cl.Location.Contains(location));
        }

        return await query.ToListAsync();
    }

    public async Task<CropListing?> GetByIdAsync(int id)
    {
        return await _context.CropListings
            .Include(cl => cl.Crop)
            .FirstOrDefaultAsync(cl => cl.Id == id);
    }

    public async Task<IEnumerable<CropListing>> GetByFarmerIdAsync(int farmerId)
    {
        return await _context.CropListings
            .Include(cl => cl.Crop)
            .Where(cl => cl.FarmerId == farmerId)
            .ToListAsync();
    }

    public async Task<CropListing> CreateAsync(CropListing listing)
    {
        _context.CropListings.Add(listing);
        await _context.SaveChangesAsync();
        return listing;
    }

    public async Task<CropListing> UpdateAsync(CropListing listing)
    {
        _context.CropListings.Update(listing);
        await _context.SaveChangesAsync();
        return listing;
    }
}
