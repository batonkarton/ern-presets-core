using LoopstationCompanionApi.Models;

namespace LoopstationCompanionApi.Dtos
{
    public record PresetUpdateDto(
        string? Name,
        DeviceModel? DeviceModel
    );
}
