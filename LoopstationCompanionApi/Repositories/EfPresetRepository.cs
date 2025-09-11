using LoopstationCompanionApi.Data;
using LoopstationCompanionApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LoopstationCompanionApi.Repositories
{
    public class EfPresetRepository(AppDbContext db) : IPresetRepository
    {
        public async Task<IReadOnlyList<PresetSummaryDto>> GetAllSummariesAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 100) pageSize = 20;

            return await db.Presets
                .AsNoTracking()
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
                .ToListAsync();
        }

        public Task<PresetDto?> GetByIdAsync(Guid id) =>
            db.Presets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id)!;

        public async Task<PresetDto> CreateAsync(PresetDto dto)
        {
            var entity = new PresetDto
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                Name = dto.Name,
                DeviceModel = dto.DeviceModel,
                UpdatedAt = dto.UpdatedAt == default ? DateTime.UtcNow : dto.UpdatedAt,
                PayloadJson = dto.PayloadJson
            };

            db.Presets.Add(entity);
            await db.SaveChangesAsync();
            return entity;
        }

        public async Task<PresetDto?> UpdateAsync(Guid id, PresetDto dto)
        {
            var entity = await db.Presets.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null) return null;

            entity.Name = dto.Name;
            entity.DeviceModel = dto.DeviceModel;
            entity.UpdatedAt = dto.UpdatedAt == default ? DateTime.UtcNow : dto.UpdatedAt;
            entity.PayloadJson = dto.PayloadJson ?? entity.PayloadJson;

            await db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await db.Presets.FindAsync(id);
            if (entity is null) return false;

            db.Presets.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<PresetDto?> UpdatePayloadAsync(Guid id, string payloadJson, DateTime updatedAt)
        {
            var entity = await db.Presets.FirstOrDefaultAsync(p => p.Id == id);
            if (entity is null) return null;

            entity.PayloadJson = payloadJson;
            entity.UpdatedAt = updatedAt == default ? DateTime.UtcNow : updatedAt;

            await db.SaveChangesAsync();
            return entity;
        }
    }
}
