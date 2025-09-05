using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Tests.Controllers.PresetController.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController;

public sealed class GetPresets : PresetsControllerTestBase
{
    [Fact]
    public async Task GetAll_UsesDefaultsAndReturnsItems()
    {
        var expected = new List<PresetSummary>();
        Service.Setup(s => s.GetAllAsync(1, 20)).ReturnsAsync(expected);

        var result = await Sut().GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        Service.Verify(s => s.GetAllAsync(1, 20), Times.Once);
    }

    [Fact]
    public async Task GetById_Found_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var preset = new Preset { Id = id, Name = "A", DeviceModel = DeviceModel.RC505mkII, UpdatedAt = DateTime.UtcNow };
        Service.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(preset);

        var result = await Sut().GetById(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(preset, ok.Value);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        Service.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((Preset?)null);

        var result = await Sut().GetById(id);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
