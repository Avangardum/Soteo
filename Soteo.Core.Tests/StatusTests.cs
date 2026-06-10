using AwesomeAssertions;
using Soteo.Core.Enums;
using Soteo.Core.Statuses;

namespace Soteo.Core.Tests;

public sealed class StatusTests
{
    [Fact]
    public void InstanceReturnsSameInstanceForEveryCall()
    {
        var instance1 = Status.Instance<TestStatus>();
        var instance2 = Status.Instance<TestStatus>();
        instance1.Should().BeSameAs(instance2);
    }
    
    [Fact]
    public void InstancingStatusWithNewThrows()
    {
        FluentActions.Invoking(() => new TestStatus()).Should().Throw<InvalidOperationException>();
    }
    
    private class TestStatus : Status
    {
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;
    }
}
