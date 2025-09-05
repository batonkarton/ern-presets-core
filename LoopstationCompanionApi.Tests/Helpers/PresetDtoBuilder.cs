using LoopstationCompanionApi.Dtos;

namespace LoopstationCompanionApi.Tests.Helpers;

public sealed class PresetDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Test";
    private string _device = "RC505mkII";
    private DateTime _updatedAt = DateTime.UtcNow.AddMinutes(-1);
    private string? _payload;

    public PresetDtoBuilder WithId(Guid id) { _id = id; return this; }
    public PresetDtoBuilder WithName(string name) { _name = name; return this; }
    public PresetDtoBuilder WithDevice(string device) { _device = device; return this; }
    public PresetDtoBuilder WithUpdatedAt(DateTime dt) { _updatedAt = dt; return this; }
    public PresetDtoBuilder WithPayload(string? json) { _payload = json; return this; }

    public PresetDto Build() => new()
    {
        Id = _id,
        Name = _name,
        DeviceModel = _device,
        UpdatedAt = _updatedAt,
        PayloadJson = _payload
    };
}
