using Microsoft.EntityFrameworkCore;
using CropService.Data;
using CropService.Models;

namespace CropService.Repositories;

public class CropRepository : ICropRepository
{
    private readonly AppDbContext _context;

    public CropRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Crop>> GetAllAsync()
    {
        return await _context.Crops.ToListAsync();
    }

    public async Task<Crop?> GetByIdAsync(int id)
    {
        return await _context.Crops.FindAsync(id);
    }

    public async Task<Crop> CreateAsync(Crop crop)
    {
        _context.Crops.Add(crop);
        await _context.SaveChangesAsync();
        return crop;
    }

    public async Task<Crop> UpdateAsync(Crop crop)
    {
        _context.Crops.Update(crop);
        await _context.SaveChangesAsync();
        return crop;
    }
}
