using LoopstationCompanionApi.Models;

namespace LoopstationCompanionApi.Dtos
{
    public record PresetDetailDto(
        Guid Id,
        string Name,
        DeviceModel DeviceModel,
        DateTime UpdatedAt,
        string JsonModel
    );

}
