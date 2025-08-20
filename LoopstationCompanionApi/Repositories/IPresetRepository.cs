using LoopstationCompanionApi.Dtos;

namespace LoopstationCompanionApi.Repositories
{
    public interface IPresetRepository
    {
        Task<IReadOnlyList<PresetDto>> GetAllAsync(int page, int pageSize);
    }
}
