using LoopstationCompanionApi.Models;
using LoopstationCompanionApi.Tests.Controllers.PresetController.Common;
using LoopstationCompanionApi.Tests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController;

public sealed class ImportPreset : PresetsControllerTestBase
{
    [Fact]
    public async Task NullRequest_BadRequest()
    {
        var bad = Assert.IsType<BadRequestObjectResult>((await Sut().ImportRc0(Guid.NewGuid(), null, default)).Result);
        Assert.Equal("File is required.", bad.Value);
    }

    [Fact]
    public async Task NullFile_BadRequest()
    {
        var bad = Assert.IsType<BadRequestObjectResult>((await Sut().ImportRc0(Guid.NewGuid(), new ImportRc0Request { File = null }, default)).Result);
        Assert.Equal("File is required.", bad.Value);
    }

    [Fact]
    public async Task WrongExtension_BadRequest_And_NoServiceCall()
    {
        var file = MockFiles.FromString("bad.txt", "x");

        var bad = Assert.IsType<BadRequestObjectResult>((await Sut().ImportRc0(Guid.NewGuid(), new ImportRc0Request { File = file }, default)).Result);
        Assert.Equal("Only .rc0 files are supported.", bad.Value);
        Service.Verify(s => s.ImportRc0Async(It.IsAny<Guid>(), It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CaseInsensitiveExtension_Success()
    {
        var id = Guid.NewGuid();
        var file = MockFiles.FromString("MEMORY083A.RC0", "<xml/>");
        var updated = new Preset { Id = id, Name = "Imported", DeviceModel = DeviceModel.RC505mkII, UpdatedAt = DateTime.UtcNow };
        Service.Setup(s => s.ImportRc0Async(id, file, It.IsAny<CancellationToken>())).ReturnsAsync(updated);

        var ok = Assert.IsType<OkObjectResult>((await Sut().ImportRc0(id, new ImportRc0Request { File = file }, default)).Result);
        Assert.Equal(updated, ok.Value);
    }

    [Fact]
    public async Task InvalidData_MapsToBadRequestWithMessage()
    {
        var id = Guid.NewGuid();
        var file = MockFiles.FromString("a.rc0", "<xml/>");
        Service.Setup(s => s.ImportRc0Async(id, file, It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidDataException("Invalid RC0 contents"));

        var result = await Sut().ImportRc0(id, new ImportRc0Request { File = file }, default);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        var dict = bad.Value!.GetType().GetProperties()
                      .ToDictionary(p => p.Name, p => p.GetValue(bad.Value));
        Assert.Equal("Invalid RC0 contents", dict["message"]);
    }

    [Fact]
    public async Task NotFound_FromService_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var file = MockFiles.FromString("a.rc0", "<xml/>");
        Service.Setup(s => s.ImportRc0Async(id, file, It.IsAny<CancellationToken>()))
               .ReturnsAsync((Preset?)null);

        var result = await Sut().ImportRc0(id, new ImportRc0Request { File = file }, default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Forwards_All_Params_To_Service()
    {
        var id = Guid.NewGuid();
        var file = MockFiles.FromString("a.rc0", "<xml/>");
        var updated = new Preset { Id = id, Name = "Imported", DeviceModel = DeviceModel.RC505mkII, UpdatedAt = DateTime.UtcNow };

        Guid? capturedId = null; IFormFile? capturedFile = null; CancellationToken capturedCt = default;
        Service.Setup(s => s.ImportRc0Async(It.IsAny<Guid>(), It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
               .Callback<Guid, IFormFile, CancellationToken>((x, f, c) => { capturedId = x; capturedFile = f; capturedCt = c; })
               .ReturnsAsync(updated);

        var ct = new CancellationTokenSource().Token;
        var ok = Assert.IsType<OkObjectResult>((await Sut().ImportRc0(id, new ImportRc0Request { File = file }, ct)).Result);

        Assert.Equal(updated, ok.Value);
        Assert.Equal(id, capturedId);
        Assert.Same(file, capturedFile);
        Assert.Equal(ct, capturedCt);
    }
}
