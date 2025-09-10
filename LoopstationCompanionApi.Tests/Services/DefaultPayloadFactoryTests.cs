using FluentAssertions;
using LoopstationCompanionApi.Services;
using LoopstationCompanionApi.Tests.Helpers;

public class DefaultPayloadFactoryTests
{
    [Fact]
    public void GetDefaultPayloadJson_Returns_Sane_Structure()
    {
        var json = DefaultPayloadFactory.GetDefaultPayloadJson();

        json.Should().Contain($"\"{TestConstants.JsonKeys.Database}\"");
        json.Should().Contain($"\"{TestConstants.JsonKeys.Ifx}\"");
        json.Should().Contain(TestConstants.Effects.AA_LPF);
        json.Should().Contain(TestConstants.Params.Rate);
        json.Should().Contain($"\"{TestConstants.JsonKeys.Text}\"");
    }

    [Fact]
    public void ApplyDefaults_Fills_Missing_And_Preserves_Provided()
    {
        var partial = DictBuilders.BuildPartialEffect(
            TestConstants.Effects.AA_LPF,
            (TestConstants.Params.Rate, "7"));

        var merged = DefaultPayloadFactory.ApplyDefaults(partial);

        merged.Should().ContainKey(TestConstants.Effects.AA_LPF);
        var lpf = (IDictionary<string, object>)merged[TestConstants.Effects.AA_LPF];

        lpf.Should().ContainKey(TestConstants.Params.Rate);
        ((IDictionary<string, string>)lpf[TestConstants.Params.Rate])[TestConstants.JsonKeys.Text]
            .Should().Be("7");

        lpf.Should().ContainKey(TestConstants.Params.Depth);
        lpf.Should().ContainKey(TestConstants.Params.Resonance);
        lpf.Should().ContainKey(TestConstants.Params.Cutoff);
        lpf.Should().ContainKey(TestConstants.Params.StepRate);
    }

    [Fact]
    public void ApplyDefaults_Preserves_Unknown_Effects()
    {
        var partial = DictBuilders.BuildUnknownEffect(
            TestConstants.Effects.MyUnknownEffect, "Some Param", "42");

        var merged = DefaultPayloadFactory.ApplyDefaults(partial);

        merged.Should().ContainKey(TestConstants.Effects.MyUnknownEffect);
    }
}
