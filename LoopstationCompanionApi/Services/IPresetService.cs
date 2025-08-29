using LoopstationCompanionApi.Models;

namespace LoopstationCompanionApi.Services
{
    public interface IPresetService
    {
        Task<IReadOnlyList<PresetSummary>> GetAllAsync(int page, int pageSize);
        Task<Preset?> GetByIdAsync(Guid id);
        Task<Preset> CreateAsync(Preset preset);
        Task<Preset?> UpdateAsync(Guid id, Preset preset);
        Task<bool> DeleteAsync(Guid id);
        Task<Preset?> ImportRc0Async(Guid id, IFormFile file, CancellationToken ct = default);
    }
}
