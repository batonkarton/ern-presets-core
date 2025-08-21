namespace LoopstationCompanionApi.Models
{
    public class CreatePresetRequest
    {
        public string Name { get; set; } = string.Empty;
        public DeviceModel DeviceModel { get; set; } = DeviceModel.RC505mkII;
    }
}
