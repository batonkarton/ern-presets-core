using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Tests.Controllers.PresetController.Common;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController;

public sealed class PostPreset : PresetsControllerTestBase
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_InvalidName_BadRequest(string? name)
    {
        var body = new CreatePresetRequest { Name = name!, DeviceModel = DeviceModel.RC505mkII };

        var result = await Sut().Create(body);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Name is required.", bad.Value);
        Service.Verify(s => s.CreateAsync(It.IsAny<Preset>()), Times.Never);
    }

    [Fact]
    public async Task Create_Valid_CreatedAtWithRouteAndForwardsValues()
    {
        var created = new Preset
        {
            Id = Guid.NewGuid(),
            Name = "New",
            DeviceModel = DeviceModel.RC505mkI,
            UpdatedAt = DateTime.UtcNow
        };
        Preset? captured = null;
        Service.Setup(s => s.CreateAsync(It.IsAny<Preset>()))
               .Callback<Preset>(p => captured = p)
               .ReturnsAsync(created);

        var result = await Sut().Create(new CreatePresetRequest { Name = "New", DeviceModel = DeviceModel.RC505mkI });

        var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(LoopstationCompanionApi.Controllers.PresetsController.GetById), createdAt.ActionName);
        Assert.Equal(created, createdAt.Value);

        Assert.Equal(created.Id, createdAt.RouteValues!["id"]);

        Assert.NotNull(captured);
        Assert.Equal("New", captured!.Name);
        Assert.Equal(DeviceModel.RC505mkI, captured.DeviceModel);
    }

}
