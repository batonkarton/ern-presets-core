namespace LoopstationCompanionApi.Dtos
{
    public record PresetListItemDto(
        Guid Id,
        string Name,
        string DeviceModel,
        DateTime UpdatedAt
    );
}
