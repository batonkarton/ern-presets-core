using LoopstationCompanionApi.Dtos;
using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Repositories;
using System.Text.Json;

namespace LoopstationCompanionApi.Services
{
    public class PresetService(IPresetRepository repo, IRc0Importer importer) : IPresetService
    {
        public async Task<IReadOnlyList<PresetSummary>> GetAllAsync(int page, int pageSize)
        {
            var dtos = await repo.GetAllSummariesAsync(page, pageSize);

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
            var dto = await repo.GetByIdAsync(id);
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
                PayloadJson = DefaultPayloadFactory.GetDefaultPayloadJson()
            };

            var saved = await repo.CreateAsync(dto);
            return MapToModel(saved);
        }

        public async Task<Preset?> UpdateAsync(Guid id, Preset preset)
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return null;

            var dto = new PresetDto
            {
                Id = id,
                Name = preset.Name,
                DeviceModel = preset.DeviceModel.ToString(),
                UpdatedAt = DateTime.UtcNow,
                PayloadJson = existing.PayloadJson
            };

            var saved = await repo.UpdateAsync(id, dto);
            return saved is null ? null : MapToModel(saved);
        }


        public Task<bool> DeleteAsync(Guid id) => repo.DeleteAsync(id);

        public async Task<Preset?> ImportRc0Async(Guid id, IFormFile file, CancellationToken ct = default)
        {
            var existing = await repo.GetByIdAsync(id);
            if (existing is null) return null;

            var payloadJson = await importer.ImportAndSanitizeAsync(file, ct);

            var updated = await repo.UpdatePayloadAsync(id, payloadJson, DateTime.UtcNow);
            return updated is null ? null : MapToModel(updated);
        }

        private static Preset MapToModel(PresetDto dto)
        {
            object? payload = null;
            if (!string.IsNullOrWhiteSpace(dto.PayloadJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(dto.PayloadJson);
                    payload = doc.RootElement.Clone();
                }
                catch
                {

                }
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