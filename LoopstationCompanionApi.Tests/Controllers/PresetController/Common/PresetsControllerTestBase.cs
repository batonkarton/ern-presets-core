using LoopstationCompanionApi.Services;
using Moq;

namespace LoopstationCompanionApi.Tests.Controllers.PresetController.Common;

public abstract class PresetsControllerTestBase
{
    protected readonly Mock<IPresetService> Service = new();
    protected LoopstationCompanionApi.Controllers.PresetsController Sut()
        => new(Service.Object);
}
