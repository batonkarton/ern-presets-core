using LoopstationCompanionApi.Dtos;
using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Repositories;
using System.Text.Json;

namespace LoopstationCompanionApi.Services
{
    public class PresetService : IPresetService
    {
        private readonly IPresetRepository _repo;
        private readonly IRc0Importer _importer;
        public PresetService(IPresetRepository repo, IRc0Importer importer)
        {
            _repo = repo;
            _importer = importer;
        }
        public async Task<IReadOnlyList<PresetSummary>> GetAllAsync(int page, int pageSize)
        {
            var dtos = await _repo.GetAllAsync(page, pageSize);
            return dtos.Select(dto =>
            {
                Enum.TryParse(dto.DeviceModel, ignoreCase: true, out DeviceModel model);
                return new PresetSummary
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    DeviceModel = model,
                    UpdatedAt = dto.UpdatedAt
                };
            }).ToList();
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
                UpdatedAt = DateTime.UtcNow,
                PayloadJson = DefaultPayloadFactory.DefaultPayloadJson()
            };

            var saved = await _repo.CreateAsync(dto);
            return MapToModel(saved);
        }

        public async Task<Preset?> UpdateAsync(Guid id, Preset preset)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing is null) return null;

            existing.Name = preset.Name;
            existing.DeviceModel = preset.DeviceModel.ToString();
            existing.UpdatedAt = DateTime.UtcNow;

            var saved = await _repo.UpdateAsync(id, existing);
            return saved is null ? null : MapToModel(saved);
        }

        public Task<bool> DeleteAsync(Guid id) => _repo.DeleteAsync(id);

        public async Task<Preset?> ImportRc0Async(Guid id, IFormFile file, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing is null) return null;

            var payloadJson = await _importer.ImportAndSanitizeAsync(file, ct);

            var updated = await _repo.UpdatePayloadAsync(id, payloadJson, DateTime.UtcNow);
            return updated is null ? null : MapToModel(updated);
        }

        private static Preset MapToModel(PresetDto dto)
        {
            object? payload = null;
            if (!string.IsNullOrWhiteSpace(dto.PayloadJson))
            {
                try { payload = JsonSerializer.Deserialize<object>(dto.PayloadJson); }
                catch { }
            }

            var model = DeviceModel.RC505mkII;
            Enum.TryParse(dto.DeviceModel, ignoreCase: true, out model);

            return new Preset
            {
                Id = dto.Id,
                Name = dto.Name,
                DeviceModel = model,
                UpdatedAt = dto.UpdatedAt,
                Payload = payload
            };
        }

        private static PresetDto MapToDto(Preset model) => new()
        {
            Id = model.Id,
            Name = model.Name,
            DeviceModel = model.DeviceModel.ToString(),
            UpdatedAt = model.UpdatedAt
        };
    }
}