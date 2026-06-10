using AwesomeAssertions;
using Soteo.Core.Abilities;
using Soteo.Core.Enums;

namespace Soteo.Core.Tests;

public sealed class AbilityTests
{
    [Fact]
    public void InstanceReturnsSameInstanceForEveryCall()
    {
        var instance1 = Ability.Instance<TestAbility>();
        var instance2 = Ability.Instance<TestAbility>();
        instance1.Should().BeSameAs(instance2);
    }
    
    [Fact]
    public void InstancingAbilityWithNewThrows()
    {
        FluentActions.Invoking(() => new TestAbility()).Should().Throw<InvalidOperationException>();
    }
    
    private class TestAbility : Ability
    {
        public override CanTarget Targeting => CanTarget.Passive;
    }
}
