using LoopstationCompanionApi.Models;

namespace LoopstationCompanionApi.Dtos
{
    public record UpdatePresetRequest(
        string? Name,
        DeviceModel? DeviceModel
    );
}
