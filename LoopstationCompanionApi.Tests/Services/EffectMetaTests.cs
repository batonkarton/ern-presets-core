using FluentAssertions;
using LoopstationCompanionApi.Services;
using LoopstationCompanionApi.UnitTests.Helpers;

namespace LoopstationCompanionApi.UnitTests.Services;

public class EffectMetaTests
{
    [Theory]
    [MemberData(nameof(EffectMetaCases.KnownParams), MemberType = typeof(EffectMetaCases))]
    public void GetParamMeta_Known_Returns_Configured_Meta(
        string effect, string key, string label, int min, int max, int def)
    {
        var m = EffectMeta.GetParamMeta(effect, key);
        m.Label.Should().Be(label);
        m.Min.Should().Be(min);
        m.Max.Should().Be(max);
        m.Default.Should().Be(def);
    }

    [Fact]
    public void GetParamMeta_Unknown_Returns_Fallback()
    {
        var m = EffectMeta.GetParamMeta("NOPE", "ZZZ");
        m.Label.Should().Be("ZZZ");
        m.Min.Should().Be(0);
        m.Max.Should().Be(127);
        m.Default.Should().Be(0);
    }

    [Fact]
    public void EffectKeys_Contains_AA_LPF()
        => EffectMeta.EffectKeys().Should().Contain(k => string.Equals(k, "AA_LPF", StringComparison.OrdinalIgnoreCase));

    [Fact]
    public void Params_For_Known_Effect_Not_Empty()
        => EffectMeta.Params("AA_LPF").Should().NotBeEmpty();
}
