namespace LoopstationCompanionApi.Dtos
{
    public class PresetDto()
    {
        public Guid Id { get; init; }
        public string Name { get; set; }
        public string DeviceModel { get; set; }
        public DateTime UpdatedAt { get; set; }
    };
}
