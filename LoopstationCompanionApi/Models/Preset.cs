namespace LoopstationCompanionApi.Models
{
    public class Preset
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public DeviceModel DeviceModel { get; set; } = DeviceModel.RC505MkII;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public string JsonModel { get; set; } = "{}";

    }
}
