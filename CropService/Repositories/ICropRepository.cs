using CropService.Models;

namespace CropService.Repositories;

public interface ICropRepository
{
    Task<IEnumerable<Crop>> GetAllAsync();
    Task<Crop?> GetByIdAsync(int id);
    Task<Crop> CreateAsync(Crop crop);
    Task<Crop> UpdateAsync(Crop crop);
}
