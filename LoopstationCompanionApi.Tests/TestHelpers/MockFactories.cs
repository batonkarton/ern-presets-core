using LoopstationCompanionApi.Repositories;
using LoopstationCompanionApi.Services;
using Moq;

namespace LoopstationCompanionApi.UnitTests.Helpers;

public static class MockFactories
{
    public static Mock<IRc0Validator> ValidatorOk()
    {
        var m = new Mock<IRc0Validator>();
        m.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync(new List<string>());
        return m;
    }

    public static Mock<IRc0Validator> ValidatorWith(params string[] errors)
    {
        var m = new Mock<IRc0Validator>();
        m.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync(errors.ToList());
        return m;
    }

    public static Mock<IPresetRepository> RepoBasic()
    {
        var r = new Mock<IPresetRepository>();
        r.Setup(x => x.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);
        return r;
    }
}
