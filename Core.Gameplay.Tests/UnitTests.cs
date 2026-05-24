using System.Reflection;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Soteo.CampaignServer;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Statuses;
using Soteo.Shared;
using Soteo.Shared.Enums;

namespace Soteo.Core.Gameplay.Tests;

public sealed class UnitTests
{
    public class StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff : Status
    {
        public const double BuffValue = 50;
        
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;

        public override IEnumerable<StatModifier> StatModifiers(StatusContext context)
        {
            // todo use ElapsedTime
            Stat stat = Maths.FloorToInt(context.DisplayElapsedTime) % 2 == 0 ? Stat.MoveSpeed : Stat.AttackSpeed;
            yield return new StatModifier(stat, StatModifierKind.Add, BuffValue);
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