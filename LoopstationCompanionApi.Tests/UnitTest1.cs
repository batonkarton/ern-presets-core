using LoopstationCompanionApi.Models;

namespace LoopstationCompanionApi.Tests;

public class UnitTest1
{
    [Fact]
    public void Preset_Should_Have_Default_DeviceModel()
    {
        var preset = new Preset { Name = "Test" };
        Assert.Equal(DeviceModel.RC505mkII, preset.DeviceModel);
    }
}