using System.Collections.Immutable;
using System.Numerics;
using AwesomeAssertions;
using NSubstitute;
using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

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

    public static readonly object?[][] ValidationSucceedsOnlyForCorrectTargetData =
    [
        [Vector2.One, false, AbilityValidationResult.Ok],
        [null, false, AbilityValidationResult.InvalidTarget],
        [Vector2.One, true, AbilityValidationResult.InvalidTarget],
        [null, true, AbilityValidationResult.Ok],
    ];

    [Theory]
    [MemberData(nameof(ValidationSucceedsOnlyForCorrectTargetData))]
    public void ValidationSucceedsOnlyForCorrectTarget
    (
        Vector2? targetPosition,
        bool alt,
        AbilityValidationResult expectedResult
    )
    {
        var context = new AbilityContext
        {
            Ability = Ability.Instance<TestAbility>(),
            Level = 1,
            User = Substitute.For<IUnit>(),
            Alt = alt,
            UserStats = ImmutableDictionary<Stat, double>.Empty,
            ServiceProvider = Substitute.For<IServiceProvider>(),
            TargetPosition = targetPosition,
            TargetUnit = null,
            TargetDirection = null,
            TargetShardId = null,
        };
        Ability.Instance<TestAbility>().Validate(context, strict: true).Should().Be(expectedResult);
    }

    private class TestAbility : Ability
    {
        public override Targeting Targeting => Targeting.Position;
        public override Targeting AltTargeting => Targeting.Nothing;
        public override Scalable<double> StaticRange => double.PositiveInfinity;
        public override Scalable<double> StaticAngularRange => double.PositiveInfinity;
    }
}
