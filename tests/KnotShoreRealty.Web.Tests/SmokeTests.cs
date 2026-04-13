using FluentAssertions;

namespace KnotShoreRealty.Web.Tests;

public class SmokeTests
{
    [Fact]
    public void TestRunnerIsWiredUp()
    {
        true.Should().BeTrue();
    }
}
