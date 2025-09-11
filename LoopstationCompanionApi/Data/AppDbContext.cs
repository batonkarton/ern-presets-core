// Data/AppDbContext.cs
using LoopstationCompanionApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LoopstationCompanionApi.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<PresetDto> Presets => Set<PresetDto>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<PresetDto>(e =>
            {
                e.ToTable("presets");
                e.HasKey(x => x.Id);

                e.Property(x => x.Id).HasColumnName("id");
                e.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
                e.Property(x => x.DeviceModel).HasColumnName("device_model").HasMaxLength(32).IsRequired();
                e.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();

                // (string in code, jsonb in DB)
                e.Property(x => x.PayloadJson)
                 .HasColumnName("payload_json")
                 .HasColumnType("jsonb");
            });
        }
    }
}
