using LoopstationCompanionApi.Models;

namespace LoopstationCompanionApi.Services
{
    public interface IPresetService
    {
        Task<IReadOnlyList<Preset>> GetAllAsync(int page, int pageSize);
    }
}
