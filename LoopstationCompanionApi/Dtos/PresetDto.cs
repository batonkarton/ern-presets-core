namespace LoopstationCompanionApi.Dtos
{
    public class PresetDto()
    {
        public Guid Id { get; init; }
        public string Name { get; set; } = string.Empty;
        public string DeviceModel { get; set; } = "RC-505mkII";
        public DateTime UpdatedAt { get; set; }
    };
}
