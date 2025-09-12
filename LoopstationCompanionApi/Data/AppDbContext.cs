using LoopstationCompanionApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LoopstationCompanionApi.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<PresetDto> Presets => Set<PresetDto>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PresetDto>(entity =>
            {
                entity.ToTable("presets");
                entity.HasKey(preset => preset.Id);

                entity.Property(preset => preset.Id)
                      .HasColumnName("id");

                entity.Property(preset => preset.Name)
                      .HasColumnName("name")
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(preset => preset.DeviceModel)
                      .HasColumnName("device_model")
                      .HasMaxLength(32)
                      .IsRequired();

                entity.Property(preset => preset.UpdatedAt)
                      .HasColumnName("updated_at")
                      .IsRequired();

                // (string in code, jsonb in DB)
                entity.Property(preset => preset.PayloadJson)
                      .HasColumnName("payload_json")
                      .HasColumnType("jsonb");
            });
        }
    }
}
