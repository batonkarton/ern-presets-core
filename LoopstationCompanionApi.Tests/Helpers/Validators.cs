using LoopstationCompanionApi.Services;
using Moq;

namespace LoopstationCompanionApi.Tests.Helpers;

public static class Validators
{
    public static Mock<IRc0Validator> AlwaysOk()
    {
        var m = new Mock<IRc0Validator>();
        m.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync(new List<string>());
        return m;
    }

    public static Mock<IRc0Validator> WithErrors(params string[] errors)
    {
        var m = new Mock<IRc0Validator>();
        m.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync(errors.ToList());
        return m;
    }
}
