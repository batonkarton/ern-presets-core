using FluentAssertions;
using LoopstationCompanionApi.Services;

namespace LoopstationCompanionApi.UnitTests.Services
{
    public class Rc0ValidatorTests
    {
        private readonly Rc0Validator _sut = new();

        [Fact]
        public async Task ValidateAsync_Passes_With_Database_Ifx_And_Effect_With_Params()
        {
            var xml = """
            <database>
              <mem id="12"></mem>
              <ifx>
                <AA_LPF>
                  <A>10</A>
                  <B>20</B>
                </AA_LPF>
              </ifx>
            </database>
            """;

            var errors = await _sut.ValidateAsync(xml);

            errors.Should().BeEmpty();
        }

        [Fact]
        public async Task ValidateAsync_Fails_When_Missing_Database()
        {
            var xml = "<root></root>";

            var errors = await _sut.ValidateAsync(xml);

            errors.Should().ContainSingle(e => e.Contains("Missing <database>"));
        }

        [Fact]
        public async Task ValidateAsync_Fails_When_Missing_Ifx()
        {
            var xml = "<database><mem id=\"1\"/></database>";

            var errors = await _sut.ValidateAsync(xml);

            errors.Should().ContainSingle(e => e.Contains("Missing <ifx>"));
        }

        [Fact]
        public async Task ValidateAsync_Fails_When_No_Effects_In_Ifx()
        {
            var xml = "<database><ifx></ifx></database>";

            var errors = await _sut.ValidateAsync(xml);

            errors.Should().ContainSingle(e => e.Contains("No effects found"));
        }

        [Fact]
        public async Task ValidateAsync_Flags_Empty_Param_Values()
        {
            var xml = """
            <database>
              <ifx>
                <AA_LPF>
                  <A></A>
                </AA_LPF>
              </ifx>
            </database>
            """;

            var errors = await _sut.ValidateAsync(xml);

            errors.Should().Contain(e => e.Contains("empty param 'A'"));
        }

        [Fact]
        public async Task ValidateAsync_Ignores__attributes_Node()
        {
            var xml = """
            <database>
              <ifx>
                <_attributes><foo>bar</foo></_attributes>
                <AA_LPF><A>1</A></AA_LPF>
              </ifx>
            </database>
            """;

            var errors = await _sut.ValidateAsync(xml);

            errors.Should().BeEmpty();
        }
    }
}
