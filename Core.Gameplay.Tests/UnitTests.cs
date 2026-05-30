using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Soteo.Core.Gameplay.Abilities;
using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Statuses;
using Soteo.Core.Shared;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Tests;

public sealed class UnitTests : Tests
{
    private readonly Sut _sut;
    private readonly IEntityManager _entityManager;
    private readonly IUnitNode _node;
    private readonly IServiceProvider _serviceProvider;

    public UnitTests()
    {
        _entityManager = Substitute.For<IEntityManager>();
        _node = Substitute.For<IUnitNode>();
        _serviceProvider = new ServiceCollection().AddSingleton(_entityManager).BuildServiceProvider();
        _sut = new Sut(Guid.NewGuid(), _node, _serviceProvider);
    }
    
    [Fact]
    public void AbilityTargetingNonexistentUnitIsNotUsed()
    {
        _sut.SetAbility<SpyAbility>(AbilitySlot.Class0, 1);
        int useCount = 0;
        Ability.Instance<SpyAbility>().Used += () => useCount++;

        _sut.SetCommand(new UseAbilityCommand(AbilitySlot.Class0, TargetUnitId: Guid.NewGuid()));
        _sut.Tick(Const.TickInterval);
        
        useCount.Should().Be(0);
    }

    [Fact]
    public void AbilityTargetingProjectileIsNotUsed()
    {
        _sut.SetAbility<SpyAbility>(AbilitySlot.Class0, 1);
        int useCount = 0;
        Ability.Instance<SpyAbility>().Used += () => useCount++;
        var projectileId = Guid.NewGuid();
        var projectileAbilityContext = new AbilityContext
        {
            Ability = Ability.Instance<SpyAbility>(),
            Level = 1,
            User = _sut,
            UserStats = _sut.Stats.ToImmutableDictionary(),
            ServiceProvider = _serviceProvider
        };
        var projectile = new Projectile
        (
            projectileId,
            projectileAbilityContext,
            speed: 1,
            Substitute.For<IProjectileNode>(),
            _serviceProvider
        );
        _entityManager.GetEntity(projectileId).Returns(projectile);
        
        _sut.SetCommand(new UseAbilityCommand(AbilitySlot.Class0, TargetUnitId: projectileId));
        _sut.Tick(Const.TickInterval);
        
        useCount.Should().Be(0);
    }

    [Fact]
    public void StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuffChangesStatsCorrectly()
    {
        _sut.AddStatus<StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff>
        (
            time: double.PositiveInfinity,
            tickInterval: null,
            source: null
        );

        const double buffValue = StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff.BuffValue;
        _sut.Stats[Stat.MoveSpeed].Should().Be(Unit.StatConst[Stat.MoveSpeed].Default + buffValue);
        _sut.Stats[Stat.AttackSpeed].Should().Be(Unit.StatConst[Stat.AttackSpeed].Default);
        
        Ticker.Tick(_sut.Tick).WithDefaultInterval().ForAtLeast(1.5);
        
        _sut.Stats[Stat.MoveSpeed].Should().Be(Unit.StatConst[Stat.MoveSpeed].Default);
        _sut.Stats[Stat.AttackSpeed].Should().Be(Unit.StatConst[Stat.AttackSpeed].Default + buffValue);
    }

    [Fact]
    public void StatusIsTickedForEveryElapsedTickInterval()
    {
        int statusTickCount = 0;
        SpyStatus.Ticked += () => statusTickCount++;
        
        _sut.AddStatus<SpyStatus>(10, 1, source: null);
        statusTickCount.Should().Be(0);
        
        Ticker.Tick(_sut.Tick).WithDefaultInterval().ForAtLeast(1);
        statusTickCount.Should().Be(1);
        
        Ticker.Tick(_sut.Tick).WithDefaultInterval().ForAtLeast(1);
        statusTickCount.Should().Be(2);
        
        Ticker.Tick(_sut.Tick).WithDefaultInterval().ForAtLeast(0.5);
        statusTickCount.Should().Be(2);
        
        Ticker.Tick(_sut.Tick).WithDefaultInterval().ForAtLeast(0.5);
        statusTickCount.Should().Be(3);
    }
    
    [Fact]
    public void StatusRemovedOnDealAttackDamageIsRemovedAccordingly()
    {
        _sut.AddStatus<StatusRemovedOnDealAttackDamage>(10, tickInterval: null, source: null);
        _sut.Statuses.Should().NotBeEmpty();
        _sut.DealAttackDamageTo(_sut, Ability.Instance<MeleeAttackAbility>());
        _sut.Statuses.Should().BeEmpty();
    }
    
    private sealed class Sut : Unit
    {
        public Sut(Guid id, IUnitNode node, IServiceProvider serviceProvider) :
            base(id, node, serviceProvider) { }
        
        public new void SetAbility<T>(AbilitySlot slot, int level) where T : Ability =>
            base.SetAbility<T>(slot, level);
    }
    
    public sealed class SpyAbility : Ability
    {
        public event Action Used = delegate { };
        
        public override CanTarget Targeting
        {
            get
            {
                return CanTarget.Character | CanTarget.Building | CanTarget.Ally | CanTarget.Enemy |
                    CanTarget.Position | CanTarget.Nothing;
            }
        }

        public override void TakeEffect(AbilityContext context)
        {
            base.TakeEffect(context);
            Used();
        }
    }
    
    public sealed class StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff : Status
    {
        public const double BuffValue = 50;
        
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;

        public override IReadOnlyList<StatModifier> StatModifiers(StatusContext context)
        {
            Stat stat = Maths.FloorToInt(context.ElapsedTime) % 2 == 0 ? Stat.MoveSpeed : Stat.AttackSpeed;
            return [new StatModifier(stat, StatModifierKind.Add, BuffValue)];
        }
    }
    
    public sealed class SpyStatus : Status
    {
        public static event Action Ticked = delegate { };
        
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;

        public override void Tick(StatusContext context, double delta)
        {
            base.Tick(context, delta);
            Ticked();
        }
    }
    
    public sealed class StatusRemovedOnDealAttackDamage : Status
    {
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;

        public override void OnDealAttackDamage(StatusContext context, Unit target, double damage)
        {
            base.OnDealAttackDamage(context, target, damage);
            context.Unit.RemoveStatus(context.Id);
        }
    }
}
