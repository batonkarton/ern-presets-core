using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Tests.Controllers.PresetController.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController;

public sealed class GetPresetsTests : PresetsControllerTestBase
{
    [Fact]
    public async Task ReturnsOk_WithItems_AndUsesDefaults_WhenGettingAll()
    {
        var controller = Sut();
        var expected = new List<PresetSummary>();
        Service.Setup(service => service.GetAllAsync(1, 20)).ReturnsAsync(expected);

        var actionResult = await controller.GetAll();
        var result = actionResult.Result;

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        Service.Verify(service => service.GetAllAsync(1, 20), Times.Once);
    }

    [Fact]
    public async Task ReturnsOk_WhenFoundById()
    {
        var controller = Sut();
        var presetId = Guid.NewGuid();
        var preset = new Preset { Id = presetId, Name = "A", DeviceModel = DeviceModel.RC505mkII, UpdatedAt = DateTime.UtcNow };
        Service.Setup(service => service.GetByIdAsync(presetId)).ReturnsAsync(preset);

        var actionResult = await controller.GetById(presetId);
        var result = actionResult.Result;

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(preset, ok.Value);
    }

    [Fact]
    public async Task ReturnsNotFound_WhenMissingById()
    {
        var controller = Sut();
        var presetId = Guid.NewGuid();
        Service.Setup(service => service.GetByIdAsync(presetId)).ReturnsAsync((Preset?)null);

        var actionResult = await controller.GetById(presetId);
        var result = actionResult.Result;

        Assert.IsType<NotFoundResult>(result);
    }
}
