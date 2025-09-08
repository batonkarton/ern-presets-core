using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Tests.Controllers.PresetController.Common;
using LoopstationCompanionApi.Tests.Helpers;
using LoopstationCompanionApi.Tests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController;

public sealed class ImportRc0Tests : PresetsControllerTestBase
{
    [Fact]
    public async Task ReturnsBadRequest_WhenRequestIsNull()
    {
        var controller = Sut();

        var actionResult = await controller.ImportRc0(Guid.NewGuid(), null, CancellationToken.None);
        var result = actionResult.Result;

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TestConstants.ValidationMessages.FileRequired, badRequest.Value);
    }

    [Fact]
    public async Task ReturnsBadRequest_WhenFileIsNull()
    {
        var controller = Sut();
        var request = new ImportRc0Request { File = null };

        var actionResult = await controller.ImportRc0(Guid.NewGuid(), request, CancellationToken.None);
        var result = actionResult.Result;

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TestConstants.ValidationMessages.FileRequired, badRequest.Value);
    }

    [Fact]
    public async Task ReturnsBadRequest_AndDoesNotCallService_WhenExtensionIsWrong()
    {
        const string badFileName = "bad.txt";
        var controller = Sut();
        var file = MockFiles.CreateFromString(badFileName, "x");
        var request = new ImportRc0Request { File = file };

        var actionResult = await controller.ImportRc0(Guid.NewGuid(), request, CancellationToken.None);
        var result = actionResult.Result;

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(TestConstants.ValidationMessages.OnlyRc0Supported, badRequest.Value);
        Service.Verify(service => service.ImportRc0Async(It.IsAny<Guid>(), It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReturnsOk_WithImportedPreset_WhenExtensionIsRc0_IgnoringCase()
    {
        const string inputFileName = "MEMORY083A.RC0";
        const string importedName = "Imported";

        var controller = Sut();
        var presetId = Guid.NewGuid();
        var file = MockFiles.CreateFromString(inputFileName, "<xml/>");
        var updated = new Preset { Id = presetId, Name = importedName, DeviceModel = DeviceModel.RC505mkII, UpdatedAt = DateTime.UtcNow };
        Service.Setup(service => service.ImportRc0Async(presetId, file, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var actionResult = await controller.ImportRc0(presetId, new ImportRc0Request { File = file }, CancellationToken.None);
        var result = actionResult.Result;

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updated, ok.Value);
    }

    [Fact]
    public async Task ReturnsBadRequest_WithMessage_WhenServiceThrowsInvalidData()
    {
        const string invalidContentsMessage = "Invalid RC0 contents";

        var controller = Sut();
        var presetId = Guid.NewGuid();
        var file = MockFiles.CreateFromString("a.rc0", "<xml/>");
        Service.Setup(service => service.ImportRc0Async(presetId, file, It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidDataException(invalidContentsMessage));

        var actionResult = await controller.ImportRc0(presetId, new ImportRc0Request { File = file }, CancellationToken.None);
        var result = actionResult.Result;

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var valueProperties = badRequest.Value!.GetType().GetProperties()
                               .ToDictionary(prop => prop.Name, prop => prop.GetValue(badRequest.Value));
        Assert.Equal(invalidContentsMessage, valueProperties["message"]);
    }

    [Fact]
    public async Task ReturnsNotFound_WhenServiceReturnsNull()
    {
        var controller = Sut();
        var presetId = Guid.NewGuid();
        var file = MockFiles.CreateFromString("a.rc0", "<xml/>");
        Service.Setup(service => service.ImportRc0Async(presetId, file, It.IsAny<CancellationToken>()))
               .ReturnsAsync((Preset?)null);

        var actionResult = await controller.ImportRc0(presetId, new ImportRc0Request { File = file }, CancellationToken.None);
        var result = actionResult.Result;

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ForwardsAllParametersToService_AndReturnsOk()
    {
        const string importedName = "Imported";

        var controller = Sut();
        var presetId = Guid.NewGuid();
        var file = MockFiles.CreateFromString("a.rc0", "<xml/>");
        var updated = new Preset { Id = presetId, Name = importedName, DeviceModel = DeviceModel.RC505mkII, UpdatedAt = DateTime.UtcNow };

        Guid? capturedId = null;
        IFormFile? capturedFile = null;
        CancellationToken capturedToken = default;

        Service.Setup(service => service.ImportRc0Async(It.IsAny<Guid>(), It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
               .Callback<Guid, IFormFile, CancellationToken>((presetIdArg, formFileArg, tokenArg) =>
               {
                   capturedId = presetIdArg;
                   capturedFile = formFileArg;
                   capturedToken = tokenArg;
               })
               .ReturnsAsync(updated);

        var cancellationToken = new CancellationTokenSource().Token;

        var actionResult = await controller.ImportRc0(presetId, new ImportRc0Request { File = file }, cancellationToken);
        var result = actionResult.Result;

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updated, ok.Value);
        Assert.Equal(presetId, capturedId);
        Assert.Same(file, capturedFile);
        Assert.Equal(cancellationToken, capturedToken);
    }
}
