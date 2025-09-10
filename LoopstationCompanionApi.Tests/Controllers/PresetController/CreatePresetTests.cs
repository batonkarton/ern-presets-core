using FluentAssertions;
using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Tests.Controllers.PresetController.Common;
using LoopstationCompanionApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController;

public sealed class CreatePresetTests : PresetsControllerTestBase
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReturnsBadRequest_WhenNameIsMissing(string? name)
    {
        var controller = Sut();
        var body = new CreatePresetRequest { Name = name!, DeviceModel = DeviceModel.RC505mkII };

        var actionResult = await controller.Create(body);
        var result = actionResult.Result;

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        badRequest.Value.Should().Be(TestConstants.ValidationMessages.NameRequired);
        Service.Verify(service => service.CreateAsync(It.IsAny<Preset>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsCreatedAt_WithRoute_AndForwardsValues_WhenValid()
    {
        const string presetName = "New";
        var controller = Sut();

        var created = new Preset
        {
            Id = Guid.NewGuid(),
            Name = presetName,
            DeviceModel = DeviceModel.RC505mkI,
            UpdatedAt = DateTime.UtcNow
        };

        Preset? capturedPreset = null;
        Service.Setup(service => service.CreateAsync(It.IsAny<Preset>()))
               .Callback<Preset>(preset => capturedPreset = preset)
               .ReturnsAsync(created);

        var actionResult = await controller.Create(new CreatePresetRequest { Name = presetName, DeviceModel = DeviceModel.RC505mkI });
        var result = actionResult.Result;

        var createdAt = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(LoopstationCompanionApi.Controllers.PresetsController.GetById), createdAt.ActionName);
        Assert.Equal(created, createdAt.Value);
        Assert.Equal(created.Id, createdAt.RouteValues!["id"]);

        Assert.NotNull(capturedPreset);
        Assert.Equal(presetName, capturedPreset!.Name);
        Assert.Equal(DeviceModel.RC505mkI, capturedPreset.DeviceModel);
    }
}
