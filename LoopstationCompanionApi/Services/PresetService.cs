using LoopstationCompanionApi.Dtos;
using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Repositories;

namespace LoopstationCompanionApi.Services
{
    public class PresetService : IPresetService
    {
        private readonly IPresetRepository _repo;
        public PresetService(IPresetRepository repo) => _repo = repo;

        public async Task<IReadOnlyList<Preset>> GetAllAsync(int page, int pageSize)
        {
            var dtos = await _repo.GetAllAsync(page, pageSize);
            return dtos.Select(MapToModel).ToList();
        }

        private static Preset MapToModel(PresetDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            DeviceModel = dto.DeviceModel,
            UpdatedAt = dto.UpdatedAt
        };

        private static PresetDto MapToDto(Preset model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            DeviceModel = model.DeviceModel,
            UpdatedAt = model.UpdatedAt
        };
    }
}
