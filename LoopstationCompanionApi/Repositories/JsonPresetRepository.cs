using LoopstationCompanionApi.Dtos;
using System.Text.Json;

namespace LoopstationCompanionApi.Repositories
{
    public class JsonPresetRepository : IPresetRepository
    {
        private readonly string _dbPath;
        private static readonly object _gate = new();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        public JsonPresetRepository(IHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(dataDir);
            _dbPath = Path.Combine(dataDir, "presets.json");
            EnsureFileWithSeed();
        }
        public async Task<IReadOnlyList<PresetSummaryDto>> GetAllSummariesAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 100) pageSize = 20;

            var all = await LoadAllAsync();

            return all
                .OrderByDescending(p => p.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PresetSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    DeviceModel = p.DeviceModel,
                    UpdatedAt = p.UpdatedAt
                })
                .ToList();
        }
        public async Task<PresetDto?> GetByIdAsync(Guid id)
        {
            var all = await LoadAllAsync();
            return all.FirstOrDefault(p => p.Id == id);
        }
        public async Task<PresetDto> CreateAsync(PresetDto dto)
        {
            var all = await LoadAllAsync();
            var toSave = new PresetDto
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                Name = dto.Name,
                DeviceModel = dto.DeviceModel,
                UpdatedAt = dto.UpdatedAt == default ? DateTime.UtcNow : dto.UpdatedAt,
                PayloadJson = dto.PayloadJson
            };
            all.Add(toSave);
            await SaveAllAsync(all);
            return toSave;
        }
        public async Task<PresetDto?> UpdateAsync(Guid id, PresetDto dto)
        {
            var all = await LoadAllAsync();
            var idx = all.FindIndex(p => p.Id == id);
            if (idx < 0) return null;

            var updated = new PresetDto
            {
                Id = id,
                Name = dto.Name,
                DeviceModel = dto.DeviceModel,
                UpdatedAt = dto.UpdatedAt == default ? DateTime.UtcNow : dto.UpdatedAt,
                PayloadJson = dto.PayloadJson ?? all[idx].PayloadJson
            };
            all[idx] = updated;
            await SaveAllAsync(all);
            return updated;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var all = await LoadAllAsync();
            var removed = all.RemoveAll(p => p.Id == id) > 0;
            if (removed) await SaveAllAsync(all);
            return removed;
        }
        public async Task<PresetDto?> UpdatePayloadAsync(Guid id, string payloadJson, DateTime updatedAt)
        {
            var all = await LoadAllAsync();
            var idx = all.FindIndex(p => p.Id == id);
            if (idx < 0) return null;

            var updated = all[idx];
            updated.PayloadJson = payloadJson;
            updated.UpdatedAt = updatedAt == default ? DateTime.UtcNow : updatedAt;

            all[idx] = updated;
            await SaveAllAsync(all);
            return updated;
        }
        private Task<List<PresetDto>> LoadAllAsync()
        {
            lock (_gate)
            {
                using var fs = File.OpenRead(_dbPath);
                var all = JsonSerializer.Deserialize<List<PresetDto>>(fs, _jsonOptions) ?? new List<PresetDto>();
                return Task.FromResult(all);
            }
        }
        private Task SaveAllAsync(List<PresetDto> presets)
        {
            lock (_gate)
            {
                var json = JsonSerializer.Serialize(presets, _jsonOptions);
                File.WriteAllText(_dbPath, json);
                return Task.CompletedTask;
            }
        }
        private void EnsureFileWithSeed()
        {
            if (File.Exists(_dbPath)) return;

            var seed = new List<PresetDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Default Vocal",
                    DeviceModel = "RC-505mkII",
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Beatbox Chain",
                    DeviceModel = "RC-505mkII",
                    UpdatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };
            var json = JsonSerializer.Serialize(seed, _jsonOptions);
            File.WriteAllText(_dbPath, json);
        }
    }
}

