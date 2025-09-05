using LoopstationCompanionApi.Tests.Controllers.PresetController.Common;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController;

public sealed class DeletePreset : PresetsControllerTestBase
{
    [Fact]
    public async Task Delete_Removed_NoContent()
    {
        var id = Guid.NewGuid();
        Service.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

        var result = await Sut().Delete(id);

        Assert.IsType<Microsoft.AspNetCore.Mvc.NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_NotFound()
    {
        var id = Guid.NewGuid();
        Service.Setup(s => s.DeleteAsync(id)).ReturnsAsync(false);

        var result = await Sut().Delete(id);

        Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>(result);
    }
}