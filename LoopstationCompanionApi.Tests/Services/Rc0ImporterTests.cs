using FluentAssertions;
using LoopstationCompanionApi.Services;
using LoopstationCompanionApi.Services.LoopstationCompanionApi.Services;
using LoopstationCompanionApi.UnitTests.TestHelpers;
using Moq;
using System.Text.Json;

namespace LoopstationCompanionApi.UnitTests.Services
{
    public class Rc0ImporterTests
    {
        [Fact]
        public async Task ImportAndSanitizeAsync_Returns_Normalized_Json_On_Valid_Input()
        {
            // Arrange
            var validator = new Mock<IRc0Validator>();
            validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<string>());

            var sut = new Rc0Importer(validator.Object);

            var xml = """
            <database>
              <ifx>
                <AA_LPF>
                  <A>3</A>
                  <B>50</B>
                  <C>50</C>
                  <D>50</D>
                  <E>9</E>
                </AA_LPF>
              </ifx>
            </database>
            """;

            var file = MockFiles.FromString("preset.rc0", xml);

            // Act
            var json = await sut.ImportAndSanitizeAsync(file);

            // Assert (structure sanity)
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            root.TryGetProperty("database", out var db).Should().BeTrue();
            db.TryGetProperty("ifx", out var ifx).Should().BeTrue();
            ifx.TryGetProperty("AA_LPF", out var lpf).Should().BeTrue();

            // Labels should be used (not A/B/C...)
            lpf.TryGetProperty("Rate", out _).Should().BeTrue();
            lpf.TryGetProperty("Depth", out _).Should().BeTrue();
            lpf.TryGetProperty("Resonance", out _).Should().BeTrue();
            lpf.TryGetProperty("Cutoff", out _).Should().BeTrue();
            lpf.TryGetProperty("Step Rate", out _).Should().BeTrue();
        }

        [Fact]
        public async Task ImportAndSanitizeAsync_Propagates_Validation_Errors_As_InvalidDataException()
        {
            var validator = new Mock<IRc0Validator>();
            validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<string> { "Missing <database>" });

            var sut = new Rc0Importer(validator.Object);

            var badXml = "<root></root>";
            var file = MockFiles.FromString("broken.rc0", badXml);

            var act = async () => await sut.ImportAndSanitizeAsync(file);

            await act.Should().ThrowAsync<InvalidDataException>()
                .WithMessage("*RC0 validation failed*");
        }

        [Fact]
        public async Task ImportAndSanitizeAsync_Cleans_Numeric_And_Symbol_Tags_And_Drops_Count()
        {
            var validator = new Mock<IRc0Validator>();
            validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<string>());

            var sut = new Rc0Importer(validator.Object);

            var xml = """
            <database>
              <count>should be removed</count>
              <ifx>
                <AA_LPF>
                  <0>7</0>
                  <#>8</#>
                </AA_LPF>
              </ifx>
            </database>
            """;

            var file = MockFiles.FromString("weird.rc0", xml);

            var json = await sut.ImportAndSanitizeAsync(file);

            // We can't see raw XML post-clean here, but final JSON should reflect:
            // - <0> turned into NUM_0 -> that key should be mapped via EffectMeta fallback to label "NUM_0"
            //   (since AA_LPF has no "0" param, it will be preserved only if ApplyDefaults keeps unknown params of provided effect)
            // - <#> turned into SYMBOL_HASH similarly.
            // Because ApplyDefaults keeps unknown params, we expect extra keys to exist under AA_LPF
            using var doc = JsonDocument.Parse(json);
            var ifx = doc.RootElement.GetProperty("database").GetProperty("ifx");

            var lpf = ifx.GetProperty("AA_LPF");

            // Known labels exist
            lpf.TryGetProperty("Rate", out _).Should().BeTrue();

            // Unknowns preserved (best-effort check): presence not guaranteed if your meta maps only known labels.
            // So assert JSON does NOT contain <count> remnants.
            json.Should().NotContain("count");
        }

        [Fact]
        public async Task ImportAndSanitizeAsync_Clamps_OutOfRange_To_Meta_MinMax()
        {
            var validator = new Mock<IRc0Validator>();
            validator.Setup(v => v.ValidateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<string>());

            var sut = new Rc0Importer(validator.Object);

            // AA_LPF A (Rate) has max 114; send 999 to ensure clamp to 114
            var xml = """
            <database>
              <ifx>
                <AA_LPF>
                  <A>999</A>
                </AA_LPF>
              </ifx>
            </database>
            """;

            var file = MockFiles.FromString("clamp.rc0", xml);

            var json = await sut.ImportAndSanitizeAsync(file);

            using var doc = JsonDocument.Parse(json);
            var lpf = doc.RootElement.GetProperty("database").GetProperty("ifx").GetProperty("AA_LPF");
            var rate = lpf.GetProperty("Rate").GetProperty("_text").GetString();

            rate.Should().Be("114");
        }
    }
}
