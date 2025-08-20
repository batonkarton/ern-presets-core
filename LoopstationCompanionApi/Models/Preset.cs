namespace LoopstationCompanionApi.Models
{
    public class Preset
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string DeviceModel { get; set; } = "RC-505mkII";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
