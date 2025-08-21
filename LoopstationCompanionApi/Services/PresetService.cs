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

        public async Task<Preset?> GetByIdAsync(Guid id)
        {
            var dto = await _repo.GetByIdAsync(id);
            return dto is null ? null : MapToModel(dto);
        }

        public async Task<Preset> CreateAsync(Preset preset)
        {
            var dto = new PresetDto
            {
                Id = Guid.NewGuid(),
                Name = preset.Name,
                DeviceModel = preset.DeviceModel.ToString(),
                UpdatedAt = DateTime.UtcNow
            };

            var saved = await _repo.CreateAsync(dto);
            return MapToModel(saved);
        }

        public async Task<Preset?> UpdateAsync(Guid id, Preset preset)
        {
            var dto = new PresetDto
            {
                Id = id,
                Name = preset.Name,
                DeviceModel = preset.DeviceModel.ToString(),
                UpdatedAt = DateTime.UtcNow
            };

            var saved = await _repo.UpdateAsync(id, dto);
            return saved is null ? null : MapToModel(saved);
        }

        public Task<bool> DeleteAsync(Guid id) => _repo.DeleteAsync(id);

        private static Preset MapToModel(PresetDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            DeviceModel = Enum.TryParse<DeviceModel>(dto.DeviceModel, out var parsed)
         ? parsed
         : DeviceModel.RC505mkII,
            UpdatedAt = dto.UpdatedAt
        };

        private static PresetDto MapToDto(Preset model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            DeviceModel = model.DeviceModel.ToString(),
            UpdatedAt = model.UpdatedAt
        };
    }
}
