using FluentAssertions;
using LoopstationCompanionApi.Services;
using LoopstationCompanionApi.Tests.Helpers;

namespace LoopstationCompanionApi.Tests.Services;

public class EffectMetaTests
{
    [Theory]
    [MemberData(nameof(EffectMetaCases.GetKnownParams), MemberType = typeof(EffectMetaCases))]
    public void GetParamMeta_Known_Returns_Configured_Meta(
        string effect, string key, string label, int min, int max, int def)
    {
        var meta = EffectMeta.GetParamMeta(effect, key);

        meta.Label.Should().Be(label);
        meta.Min.Should().Be(min);
        meta.Max.Should().Be(max);
        meta.Default.Should().Be(def);
    }

    [Fact]
    public void GetParamMeta_Unknown_Returns_Fallback()
    {
        const string unknownEffect = "NOPE";
        const string unknownParam = "ZZZ";

        var meta = EffectMeta.GetParamMeta(unknownEffect, unknownParam);

        meta.Label.Should().Be(unknownParam);
        meta.Min.Should().Be(0);
        meta.Max.Should().Be(127);
        meta.Default.Should().Be(0);
    }

    [Fact]
    public void EffectKeys_Contains_AA_LPF()
        => EffectMeta.EffectKeys()
            .Should().Contain(effectKey =>
                string.Equals(effectKey, TestConstants.Effects.AA_LPF, StringComparison.OrdinalIgnoreCase));

    [Fact]
    public void Params_For_Known_Effect_Not_Empty()
        => EffectMeta.Params(TestConstants.Effects.AA_LPF).Should().NotBeEmpty();
}
