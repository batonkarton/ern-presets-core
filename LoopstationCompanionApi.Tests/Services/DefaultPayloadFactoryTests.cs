using FluentAssertions;
using LoopstationCompanionApi.Services;
using System.Text.Json;

namespace LoopstationCompanionApi.UnitTests.Services
{
    public class DefaultPayloadFactoryTests
    {
        [Fact]
        public void DefaultPayloadObject_Returns_All_Effects_With_Default_Param_Texts()
        {
            var obj = DefaultPayloadFactory.DefaultPayloadObject();
            var json = JsonSerializer.Serialize(obj);

            json.Should().Contain("\"database\"");
            json.Should().Contain("\"ifx\"");

            json.Should().Contain("AA_LPF");
            json.Should().Contain("Rate");
            json.Should().Contain("\"_text\"");
        }

        [Fact]
        public void ApplyDefaults_Fills_Missing_Params_And_Preserves_Provided_Values()
        {
            var partial = new Dictionary<string, object>
            {
                ["AA_LPF"] = new Dictionary<string, object>
                {
                    // Provided "Rate" = 7 (should be kept)
                    ["Rate"] = new Dictionary<string, string> { ["_text"] = "7" }
                    // Missing Depth/Resonance/Cutoff/Step Rate -> should be filled with defaults
                }
            };

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
            var partial = new Dictionary<string, object>
            {
                ["MY_UNKNOWN_EFFECT"] = new Dictionary<string, object>
                {
                    ["Some Param"] = new Dictionary<string, string> { ["_text"] = "42" }
                }
            };

            var merged = DefaultPayloadFactory.ApplyDefaults(partial);

            merged.Should().ContainKey("MY_UNKNOWN_EFFECT");
        }
    }
}
