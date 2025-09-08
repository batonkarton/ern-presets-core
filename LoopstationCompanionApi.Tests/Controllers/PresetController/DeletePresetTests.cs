using LoopstationCompanionApi.Tests.Controllers.PresetController.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController;

public sealed class DeletePresetTests : PresetsControllerTestBase
{
    [Fact]
    public async Task ReturnsNoContent_WhenDeleted()
    {
        var controller = Sut();
        var presetId = Guid.NewGuid();
        Service.Setup(service => service.DeleteAsync(presetId)).ReturnsAsync(true);

        var result = await controller.Delete(presetId);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ReturnsNotFound_WhenDeleteReturnsFalse()
    {
        var controller = Sut();
        var presetId = Guid.NewGuid();
        Service.Setup(service => service.DeleteAsync(presetId)).ReturnsAsync(false);

        var result = await controller.Delete(presetId);

        Assert.IsType<NotFoundResult>(result);
    }
}
