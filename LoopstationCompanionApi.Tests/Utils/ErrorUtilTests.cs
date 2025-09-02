using FluentAssertions;
using LoopstationCompanionApi.Services;

namespace LoopstationCompanionApi.Tests.Utils
{
    public class ErrorUtilTests
    {
        [Fact]
        public void JoinMessages_Joins_With_Semicolon_Space_By_Default()
        {
            var joined = ErrorUtils.JoinMessages(["A", "B", "C"]);
            joined.Should().Be("A; B; C");
        }

        [Fact]
        public void JoinMessages_Returns_Empty_When_No_Errors()
        {
            var joined = ErrorUtils.JoinMessages([]);
            joined.Should().BeEmpty();
        }
    }
}
