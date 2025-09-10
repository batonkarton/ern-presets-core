using LoopstationCompanionApi.Repositories;
using LoopstationCompanionApi.Services;
using Moq;

namespace LoopstationCompanionApi.Tests.Mocks;

public static class MockFactories
{
    public static Mock<IRc0Validator> CreateValidatorThatAlwaysSucceeds()
    {
        var validatorMock = new Mock<IRc0Validator>();
        validatorMock
            .Setup(validator => validator.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());
        return validatorMock;
    }

    public static Mock<IRc0Validator> CreateValidatorWithErrors(params string[] errors)
    {
        var validatorMock = new Mock<IRc0Validator>();
        validatorMock
            .Setup(validator => validator.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(errors.ToList());
        return validatorMock;
    }

    public static Mock<IPresetRepository> CreatePresetRepositoryWithDeleteAlwaysTrue()
    {
        var repoMock = new Mock<IPresetRepository>();
        repoMock
            .Setup(repo => repo.DeleteAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);
        return repoMock;
    }

    public static (PresetService sut,
        Mock<IPresetRepository> repo,
        Mock<IRc0Importer> importer)
        CreatePresetService(Action<Mock<IPresetRepository>>? configureRepo = null, Action<Mock<IRc0Importer>>? configureImporter = null)
    {
        var repoMock = new Mock<IPresetRepository>();
        var importerMock = new Mock<IRc0Importer>();

        configureRepo?.Invoke(repoMock);
        configureImporter?.Invoke(importerMock);

        var sut = new PresetService(repoMock.Object, importerMock.Object);
        return (sut, repoMock, importerMock);
    }
}
