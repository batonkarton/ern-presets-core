using FluentAssertions;
using LoopstationCompanionApi.Services;
using LoopstationCompanionApi.UnitTests.Helpers;

public class Rc0ValidatorTests
{
    private readonly Rc0Validator _sut = new();

    [Fact]
    public async Task ValidateAsync_Passes_With_Database_Ifx_And_Effect_With_Params()
    {
        var errors = await _sut.ValidateAsync(XmlSamples.ValidDatabaseWithIfxAndParams);
        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_Fails_When_Missing_Database()
    {
        var errors = await _sut.ValidateAsync(XmlSamples.MissingDatabase);
        errors.Should().ContainSingle(e => e.Contains("Missing <database>"));
    }

    [Fact]
    public async Task ValidateAsync_Fails_When_Missing_Ifx()
    {
        var errors = await _sut.ValidateAsync(XmlSamples.MissingIfx);
        errors.Should().ContainSingle(e => e.Contains("Missing <ifx>"));
    }

    [Fact]
    public async Task ValidateAsync_Fails_When_No_Effects_In_Ifx()
    {
        var errors = await _sut.ValidateAsync(XmlSamples.EmptyIfx);
        errors.Should().ContainSingle(e => e.Contains("No effects found"));
    }

    [Fact]
    public async Task ValidateAsync_Flags_Empty_Param_Values()
    {
        var errors = await _sut.ValidateAsync(XmlSamples.EmptyParamA);
        errors.Should().Contain(e => e.Contains("empty param 'A'"));
    }

    [Fact]
    public async Task ValidateAsync_Ignores__attributes_Node()
    {
        var errors = await _sut.ValidateAsync(XmlSamples.AttributesNodePresent);
        errors.Should().BeEmpty();
    }
}
