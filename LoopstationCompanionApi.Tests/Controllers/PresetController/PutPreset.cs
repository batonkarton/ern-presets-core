using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Tests.Controllers.PresetController.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController;

public sealed class PutPreset : PresetsControllerTestBase
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Update_InvalidName_BadRequest(string? name)
    {
        var result = await Sut().Update(Guid.NewGuid(), new UpdatePresetRequest { Name = name!, DeviceModel = DeviceModel.RC505mkII });

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Name is required.", bad.Value);
        Service.Verify(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Preset>()), Times.Never);
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        Service.Setup(s => s.UpdateAsync(id, It.IsAny<Preset>())).ReturnsAsync((Preset?)null);

        var result = await Sut().Update(id, new UpdatePresetRequest { Name = "X", DeviceModel = DeviceModel.RC505mkI });

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_Valid_Ok_And_ForwardsValues()
    {
        var id = Guid.NewGuid();
        var updated = new Preset { Id = id, Name = "Updated", DeviceModel = DeviceModel.RC505mkI, UpdatedAt = DateTime.UtcNow };
        Preset? captured = null;
        Service.Setup(s => s.UpdateAsync(id, It.IsAny<Preset>()))
               .Callback<Guid, Preset>((_, p) => captured = p)
               .ReturnsAsync(updated);

        var result = await Sut().Update(id, new UpdatePresetRequest { Name = "Updated", DeviceModel = DeviceModel.RC505mkI });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(updated, ok.Value);
        Assert.NotNull(captured);
        Assert.Equal("Updated", captured!.Name);
        Assert.Equal(DeviceModel.RC505mkI, captured.DeviceModel);
    }
}
