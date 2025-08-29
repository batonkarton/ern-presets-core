namespace LoopstationCompanionApi.Models
{
    public class PresetSummary
    {
        public Guid Id { get; init; }
        public string Name { get; set; } = string.Empty;
        public DeviceModel DeviceModel { get; set; } = DeviceModel.RC505mkII;
        public DateTime UpdatedAt { get; set; }
    }
}
