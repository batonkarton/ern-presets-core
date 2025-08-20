using LoopstationCompanionApi.Models;

namespace LoopstationCompanionApi.Dtos
{
    public record CreatePresetRequest(
        string Name,
        DeviceModel DeviceModel = DeviceModel.RC505MkII
    );
}
