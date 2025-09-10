using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Tests.Controllers.PresetController.Common;
using LoopstationCompanionApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController;

public sealed class UpdatePresetTests : PresetsControllerTestBase
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReturnsBadRequest_WhenNameIsMissing(string? name)
    {
        var controller = Sut();

        var actionResult = await controller.Update(Guid.NewGuid(), new UpdatePresetRequest { Name = name!, DeviceModel = DeviceModel.RC505mkII });
        var result = actionResult.Result;

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TestConstants.ValidationMessages.NameRequired, badRequest.Value);
        Service.Verify(service => service.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Preset>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsNotFound_WhenServiceReturnsNull()
    {
        var controller = Sut();
        var presetId = Guid.NewGuid();
        Service.Setup(service => service.UpdateAsync(presetId, It.IsAny<Preset>())).ReturnsAsync((Preset?)null);

        var actionResult = await controller.Update(presetId, new UpdatePresetRequest { Name = "X", DeviceModel = DeviceModel.RC505mkI });
        var result = actionResult.Result;

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ReturnsOk_AndForwardsValues_WhenValid()
    {
        const string updatedName = "Updated";

        var controller = Sut();
        var presetId = Guid.NewGuid();
        var updated = new Preset { Id = presetId, Name = updatedName, DeviceModel = DeviceModel.RC505mkI, UpdatedAt = DateTime.UtcNow };
        Preset? capturedPreset = null;

        Service.Setup(service => service.UpdateAsync(presetId, It.IsAny<Preset>()))
               .Callback<Guid, Preset>((_, preset) => capturedPreset = preset)
               .ReturnsAsync(updated);

        var actionResult = await controller.Update(presetId, new UpdatePresetRequest { Name = updatedName, DeviceModel = DeviceModel.RC505mkI });
        var result = actionResult.Result;

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updated, ok.Value);
        Assert.NotNull(capturedPreset);
        Assert.Equal(updatedName, capturedPreset!.Name);
        Assert.Equal(DeviceModel.RC505mkI, capturedPreset.DeviceModel);
    }
}
