using FluentAssertions;
using LoopstationCompanionApi.Services.LoopstationCompanionApi.Services;
using LoopstationCompanionApi.Tests.Helpers;
using LoopstationCompanionApi.Tests.Mocks;
using System.Text.Json;

public class Rc0ImporterTests
{
    [Fact]
    public async Task ImportAndSanitizeAsync_Returns_Normalized_Json_On_Valid_Input()
    {
        const string inputFileName = "preset.rc0";

        var validator = MockFactories.CreateValidatorThatAlwaysSucceeds();
        var sut = new Rc0Importer(validator.Object);

        var file = MockFiles.CreateFromString(inputFileName, XmlSamples.ValidImporterAA_LPF);

        var resultJson = await sut.ImportAndSanitizeAsync(file);

        using var document = JsonDocument.Parse(resultJson);
        var ifxElement = document.RootElement
                                 .GetProperty(TestConstants.JsonKeys.Database)
                                 .GetProperty(TestConstants.JsonKeys.Ifx);
        var lpfElement = ifxElement.GetProperty(TestConstants.Effects.AA_LPF);

        lpfElement.TryGetProperty(TestConstants.Params.Rate, out _).Should().BeTrue();
        lpfElement.TryGetProperty(TestConstants.Params.Depth, out _).Should().BeTrue();
        lpfElement.TryGetProperty(TestConstants.Params.Resonance, out _).Should().BeTrue();
        lpfElement.TryGetProperty(TestConstants.Params.Cutoff, out _).Should().BeTrue();
        lpfElement.TryGetProperty(TestConstants.Params.StepRate, out _).Should().BeTrue();
    }

    [Fact]
    public async Task ImportAndSanitizeAsync_Propagates_Validation_Errors_As_InvalidDataException()
    {
        const string inputFileName = "broken.rc0";
        const string missingDatabaseError = "Missing <database>";

        var validator = MockFactories.CreateValidatorWithErrors(missingDatabaseError);
        var sut = new Rc0Importer(validator.Object);

        var file = MockFiles.CreateFromString(inputFileName, XmlSamples.MissingDatabase);
        var actAsync = async () => await sut.ImportAndSanitizeAsync(file);

        await actAsync.Should().ThrowAsync<InvalidDataException>()
                      .WithMessage($"*{TestConstants.ValidationMessages.Rc0ValidationFailedContains}*");
    }

    [Fact]
    public async Task ImportAndSanitizeAsync_Cleans_Numeric_And_Symbol_Tags_And_Drops_Count()
    {
        const string inputFileName = "weird.rc0";
        const string removedNodeName = "count";

        var validator = MockFactories.CreateValidatorThatAlwaysSucceeds();
        var sut = new Rc0Importer(validator.Object);

        var file = MockFiles.CreateFromString(inputFileName, XmlSamples.WeirdNumericAndSymbolAndCount);

        var resultJson = await sut.ImportAndSanitizeAsync(file);

        using var document = JsonDocument.Parse(resultJson);
        var lpfElement = document.RootElement
                                 .GetProperty(TestConstants.JsonKeys.Database)
                                 .GetProperty(TestConstants.JsonKeys.Ifx)
                                 .GetProperty(TestConstants.Effects.AA_LPF);

        lpfElement.TryGetProperty(TestConstants.Params.Rate, out _).Should().BeTrue();
        resultJson.Should().NotContain(removedNodeName);
    }

    [Fact]
    public async Task ImportAndSanitizeAsync_Clamps_OutOfRange_To_Meta_MinMax()
    {
        const string inputFileName = "clamp.rc0";
        const string expectedClampedRate = "114";

        var validator = MockFactories.CreateValidatorThatAlwaysSucceeds();
        var sut = new Rc0Importer(validator.Object);

        var file = MockFiles.CreateFromString(inputFileName, XmlSamples.Clamp_AA_LPF_A_TooHigh);

        var resultJson = await sut.ImportAndSanitizeAsync(file);

        using var document = JsonDocument.Parse(resultJson);
        var rateValue = document.RootElement
                                .GetProperty(TestConstants.JsonKeys.Database)
                                .GetProperty(TestConstants.JsonKeys.Ifx)
                                .GetProperty(TestConstants.Effects.AA_LPF)
                                .GetProperty(TestConstants.Params.Rate)
                                .GetProperty(TestConstants.JsonKeys.Text)
                                .GetString();

        rateValue.Should().Be(expectedClampedRate);
    }
}
