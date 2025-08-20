using LoopstationCompanionApi.Dtos;
using System.Text.Json;

namespace LoopstationCompanionApi.Repositories
{
    public class JsonPresetRepository : IPresetRepository
    {
        private readonly string _dbPath;
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

        public async Task<IReadOnlyList<PresetDto>> GetAllAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 100) pageSize = 20;

            await using var fs = File.OpenRead(_dbPath);
            var all = await JsonSerializer.DeserializeAsync<List<PresetDto>>(fs, _jsonOptions)
                      ?? new List<PresetDto>();

            var items = all
                .OrderByDescending(p => p.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return items;
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

