using System.Reflection;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Statuses;
using Soteo.Core.Shared;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Tests;

public sealed class UnitTests
{
    public class StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff : Status
    {
        public const double BuffValue = 50;
        
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;

        public override IReadOnlyList<StatModifier> StatModifiers(StatusContext context)
        {
            Stat stat = Maths.FloorToInt(context.ElapsedTime) % 2 == 0 ? Stat.MoveSpeed : Stat.AttackSpeed;
            return [new StatModifier(stat, StatModifierKind.Add, BuffValue)];
        }
    }
 
    static UnitTests()
    {
        TypeLocator.Init(Assembly.GetExecutingAssembly());
    }
    
    [Fact]
    public void StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuffChangesStatsCorrectly()
    {
        IServiceProvider serviceProvider =
            new ServiceCollection().AddSubstitute<IEntityManager>().BuildServiceProvider();
        IUnitNode node = Substitute.For<IUnitNode>();
        var sut = new Unit(Guid.NewGuid(), node, serviceProvider);
        
        sut.AddStatus<StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff>
        (
            time: double.PositiveInfinity,
            tickInterval: 0,
            abilityContext: null,
            source: null
        );

        const double buffValue = StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff.BuffValue;
        sut.Stats[Stat.MoveSpeed].Should().Be(Unit.StatConst[Stat.MoveSpeed].Default + buffValue);
        sut.Stats[Stat.AttackSpeed].Should().Be(Unit.StatConst[Stat.AttackSpeed].Default);
        
        Ticker.Tick(sut.PhysicsProcess).WithDefaultInterval().ForAtLeast(1.5);
        
        sut.Stats[Stat.MoveSpeed].Should().Be(Unit.StatConst[Stat.MoveSpeed].Default);
        sut.Stats[Stat.AttackSpeed].Should().Be(Unit.StatConst[Stat.AttackSpeed].Default + buffValue);
    }
}