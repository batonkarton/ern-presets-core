using LoopstationCompanionApi.Models;

namespace LoopstationCompanionApi.Services
{
    public interface IPresetService
    {
        Task<IReadOnlyList<Preset>> GetAllAsync(int page, int pageSize);
        Task<Preset?> GetByIdAsync(Guid id);
        Task<Preset> CreateAsync(Preset preset);
        Task<Preset?> UpdateAsync(Guid id, Preset preset);
        Task<bool> DeleteAsync(Guid id);
    }
}
