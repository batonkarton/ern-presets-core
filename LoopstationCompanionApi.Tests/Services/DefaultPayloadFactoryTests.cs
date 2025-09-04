using FluentAssertions;
using LoopstationCompanionApi.Services;
using LoopstationCompanionApi.Tests.Helpers;

public class DefaultPayloadFactoryTests
{
    [Fact]
    public void GetDefaultPayloadJson_Returns_Sane_Structure()
    {
        var json = DefaultPayloadFactory.GetDefaultPayloadJson();

        json.Should().Contain("\"database\"");
        json.Should().Contain("\"ifx\"");
        json.Should().Contain("AA_LPF");
        json.Should().Contain("Rate");
        json.Should().Contain("\"_text\"");
    }

    [Fact]
    public void ApplyDefaults_Fills_Missing_And_Preserves_Provided()
    {
        var partial = DictBuilders.PartialEffect("AA_LPF", ("Rate", "7"));

        var merged = DefaultPayloadFactory.ApplyDefaults(partial);

        merged.Should().ContainKey("AA_LPF");
        var lpf = (IDictionary<string, object>)merged["AA_LPF"];

        lpf.Should().ContainKey("Rate");
        ((IDictionary<string, string>)lpf["Rate"])["_text"].Should().Be("7");

        lpf.Should().ContainKey("Depth");
        lpf.Should().ContainKey("Resonance");
        lpf.Should().ContainKey("Cutoff");
        lpf.Should().ContainKey("Step Rate");
    }

    [Fact]
    public void ApplyDefaults_Preserves_Unknown_Effects()
    {
        var partial = DictBuilders.UnknownEffect("MY_UNKNOWN_EFFECT", "Some Param", "42");

        var merged = DefaultPayloadFactory.ApplyDefaults(partial);

        merged.Should().ContainKey("MY_UNKNOWN_EFFECT");
    }
}
