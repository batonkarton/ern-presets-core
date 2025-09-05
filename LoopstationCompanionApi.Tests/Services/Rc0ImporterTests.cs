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
        var validator = Validators.AlwaysOk();
        var sut = new Rc0Importer(validator.Object);

        var file = MockFiles.FromString("preset.rc0", XmlSamples.ValidImporterAA_LPF);

        var json = await sut.ImportAndSanitizeAsync(file);

        using var doc = JsonDocument.Parse(json);
        var ifx = doc.RootElement.GetProperty("database").GetProperty("ifx");
        var lpf = ifx.GetProperty("AA_LPF");

        lpf.TryGetProperty("Rate", out _).Should().BeTrue();
        lpf.TryGetProperty("Depth", out _).Should().BeTrue();
        lpf.TryGetProperty("Resonance", out _).Should().BeTrue();
        lpf.TryGetProperty("Cutoff", out _).Should().BeTrue();
        lpf.TryGetProperty("Step Rate", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ImportAndSanitizeAsync_Propagates_Validation_Errors_As_InvalidDataException()
    {
        var validator = Validators.WithErrors("Missing <database>");
        var sut = new Rc0Importer(validator.Object);

        var file = MockFiles.FromString("broken.rc0", XmlSamples.MissingDatabase);
        var act = async () => await sut.ImportAndSanitizeAsync(file);

        await act.Should().ThrowAsync<InvalidDataException>()
                 .WithMessage("*RC0 validation failed*");
    }

    [Fact]
    public async Task ImportAndSanitizeAsync_Cleans_Numeric_And_Symbol_Tags_And_Drops_Count()
    {
        var validator = Validators.AlwaysOk();
        var sut = new Rc0Importer(validator.Object);

        var file = MockFiles.FromString("weird.rc0", XmlSamples.WeirdNumericAndSymbolAndCount);

        var json = await sut.ImportAndSanitizeAsync(file);

        using var doc = JsonDocument.Parse(json);
        var lpf = doc.RootElement.GetProperty("database").GetProperty("ifx").GetProperty("AA_LPF");

        lpf.TryGetProperty("Rate", out _).Should().BeTrue();
        json.Should().NotContain("count");
    }

    [Fact]
    public async Task ImportAndSanitizeAsync_Clamps_OutOfRange_To_Meta_MinMax()
    {
        var validator = Validators.AlwaysOk();
        var sut = new Rc0Importer(validator.Object);

        var file = MockFiles.FromString("clamp.rc0", XmlSamples.Clamp_AA_LPF_A_TooHigh);

        var json = await sut.ImportAndSanitizeAsync(file);

        using var doc = JsonDocument.Parse(json);
        var rate = doc.RootElement
                      .GetProperty("database")
                      .GetProperty("ifx")
                      .GetProperty("AA_LPF")
                      .GetProperty("Rate")
                      .GetProperty("_text")
                      .GetString();

        rate.Should().Be("114");
    }
}
