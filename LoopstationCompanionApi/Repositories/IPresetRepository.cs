using LoopstationCompanionApi.Dtos;

namespace LoopstationCompanionApi.Repositories
{
    public interface IPresetRepository
    {
        Task<IReadOnlyList<PresetDto>> GetAllAsync(int page, int pageSize);
        Task<PresetDto?> GetByIdAsync(Guid id);
        Task<PresetDto> CreateAsync(PresetDto dto);
        Task<PresetDto?> UpdateAsync(Guid id, PresetDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
