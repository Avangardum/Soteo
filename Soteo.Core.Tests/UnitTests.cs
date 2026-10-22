using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Soteo.Core.Abilities;
using Soteo.Core.Commands;
using Soteo.Core.Dto;
using Soteo.Core.Entities;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.Items;
using Soteo.Core.StaticHelpers;
using Soteo.Core.Statuses;
using Soteo.TestUtil;
using Soteo.Util;

namespace Soteo.Core.Tests;

public sealed class UnitTests
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
        _sut = new Sut(Guid.NewGuid(), _node, _entityManager, _serviceProvider);
    }

    [Fact]
    public void UseAbilityCommandForEmptySlotDoesntThrow()
    {
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Class0, TargetUnitId = Guid.NewGuid() });
        _sut.Tick(Const.TickInterval);
    }

    [Fact]
    public void AbilityTargetingNonexistentUnitIsNotUsed()
    {
        _sut.SetAbility<SpyAbility>(AbilitySlot.Class0, 1);
        int useCount = 0;
        Ability.Instance<SpyAbility>().Used += () => useCount++;

        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Class0, TargetUnitId = Guid.NewGuid() });
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
            target: new ProjectileTarget(_sut),
            Substitute.For<IProjectileNode>(),
            _serviceProvider
        );
        _entityManager.Entities.Returns(new Dictionary<Guid, IEntity> { [projectileId] = projectile });

        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Class0, TargetUnitId = projectileId });
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

        _sut.AddStatus<SpyStatus>(time: 10, tickInterval: 1, source: null);
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
        _sut.AddStatus<StatusRemovedOnDealAttackDamage>(time: 10, tickInterval: null, source: null);
        _sut.Statuses.Should().NotBeEmpty();
        _sut.DealAttackDamageTo(_sut, Ability.Instance<MeleeAttackAbility>());
        _sut.Statuses.Should().BeEmpty();
    }

    [Fact]
    public void DieOnTickStatusKillsItsUnitWhenTicked()
    {
        _sut.AddStatus<DieOnTickStatus>(time: 10, tickInterval: 1, source: null);
        Ticker.Tick(_sut.Tick).WithDefaultInterval().ForAtLeast(1);
        _sut.IsDead.Should().BeTrue();
    }

    [Fact]
    public void UsingAbilityValidatesItStrictlyInitiallyThenNonStrictlyWhileUsing()
    {
        _sut.SetAbility<SpyAbilityWithUseTime>(AbilitySlot.Class0, 1);
        int validatedStrictlyCount = 0;
        int validatedNonStrictlyCount = 0;
        Ability.Instance<SpyAbilityWithUseTime>().ValidatedStrictly += () => validatedStrictlyCount++;
        Ability.Instance<SpyAbilityWithUseTime>().ValidatedNonStrictly += () => validatedNonStrictlyCount++;

        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Class0 });
        _sut.Tick(Const.TickInterval);

        validatedStrictlyCount.Should().Be(1);
        validatedNonStrictlyCount.Should().Be(0);

        _sut.Tick(Const.TickInterval);

        validatedStrictlyCount.Should().Be(1);
        validatedNonStrictlyCount.Should().Be(1);

        _sut.Tick(Const.TickInterval);

        validatedStrictlyCount.Should().Be(1);
        validatedNonStrictlyCount.Should().Be(2);
    }

    [Fact]
    public void UsingAbilityWhileAnotherAbilityIsBeingUsedValidatesItStrictlyInitiallyThenNonStrictlyWhileUsing()
    {
        _sut.SetAbility<AbilityWithUseTime>(AbilitySlot.Class1, 1);
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Class1 });
        _sut.Tick(Const.TickInterval);

        UsingAbilityValidatesItStrictlyInitiallyThenNonStrictlyWhileUsing();
    }

    [Fact]
    public void MovingItemStackFromInventorySlotToAnotherInventorySlotMovesItThereAndMovesExistingStackToSourceSlot()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestItem1>(1));
        _sut.Inventory.PutAt(1, ItemStack.Of<TestItem2>(1));

        _sut.SetCommand(new MoveItemStackCommand { FromIndex = 0, ToIndex = 1 });
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestItem2>(1));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestItem1>(1));
    }

    [Fact]
    public void MovingItemStackFromEmptyInventorySlotDoesNothing()
    {
        _sut.SetCommand(new MoveItemStackCommand { FromIndex = 0, ToIndex = 1 });
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().BeNull();
        _sut.Inventory[1].Should().BeNull();
    }

    [Fact]
    public void MovingItemStackFromInvalidInventorySlotDoesNothing()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestItem1>(1));
        _sut.Inventory.PutAt(1, ItemStack.Of<TestItem2>(1));

        _sut.SetCommand(new MoveItemStackCommand { FromIndex = -228, ToIndex = 1 });
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestItem1>(1));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestItem2>(1));
    }

    [Fact]
    public void MovingItemStackToInvalidInventorySlotDoesNothing()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestItem1>(1));
        _sut.Inventory.PutAt(1, ItemStack.Of<TestItem2>(1));

        _sut.SetCommand(new MoveItemStackCommand { FromIndex = 0, ToIndex = int.MaxValue });
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestItem1>(1));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestItem2>(1));
    }

    [Fact]
    public void EquippingUnequippableItemDoesNothing()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestItem1>(1));

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestItem1>(1));
        _sut.EquipmentSlotStates.All(it => it.Value.ItemStack == null).Should().BeTrue();
    }

    [Fact]
    public void EquippingIntoOccupiedEquipmentSlotDoesNothing()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestPotion2Item>(1));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion] =
            new EquipmentSlotState { ItemStack = ItemStack.Of<TestPotion1Item>(1) };

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion2Item>(1));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().Be(ItemStack.Of<TestPotion1Item>(1));
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void EquippingFromEmptyOrNonexistentInventorySlotDoesNothing(int inventorySlot)
    {
        _sut.SetCommand(new EquipCommand(inventorySlot));
        _sut.Tick(Const.TickInterval);

        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().BeNull();
    }

    [Fact]
    public void EquippingToNonexistentEquipmentSlotDoesNothing()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestHatItem>(1));

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestHatItem>(1));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().BeNull();
    }

    [Fact]
    public void EquippingItemStackToEmptyEquipmentSlotMovesItToThatSlot()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestPotion1Item>(1));

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().BeNull();
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().Be(ItemStack.Of<TestPotion1Item>(1));
    }

    [Fact]
    public void EquippingItemStackToEquipmentSlotOccupiedBySameItemEquipsWhatIsPossibleAndLeavesRestInInventory()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestPotion1Item>(5));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion] =
            new EquipmentSlotState { ItemStack = ItemStack.Of<TestPotion1Item>(8) };

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion1Item>(3));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().Be(ItemStack.Of<TestPotion1Item>(10));
    }

    [Theory]
    [InlineData(EquipmentSlot.Head)]
    [InlineData(EquipmentSlot.Potion)]
    public void UnequippingItemFromNonexistentOrEmptyEquipmentSlotDoesNothing(EquipmentSlot slot)
    {
        _sut.SetCommand(new UnequipCommand(slot));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory.WhereNotNull().Should().BeEmpty();
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().BeNull();
    }

    [Fact]
    public void UnequippingItemWhenInventoryHasNoSpaceDoesNothing()
    {
        for (int i = 0; i < _sut.Inventory.Count; i++)
            _sut.Inventory.PutAt(i, ItemStack.Of<TestHatItem>(1));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion] =
            new EquipmentSlotState { ItemStack = ItemStack.Of<TestPotion1Item>(1) };

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory.All(it => it == ItemStack.Of<TestHatItem>(1)).Should().BeTrue();
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().Be(ItemStack.Of<TestPotion1Item>(1));
    }

    [Fact]
    public void UnequippingItemFillsAvailableSpaceMergingStacksIfPossible()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestPotion1Item>(9));
        _sut.Inventory.PutAt(1, ItemStack.Of<TestPotion1Item>(8));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion] =
            new EquipmentSlotState { ItemStack = ItemStack.Of<TestPotion1Item>(5) };

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion1Item>(10));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestPotion1Item>(10));
        _sut.Inventory[2].Should().Be(ItemStack.Of<TestPotion1Item>(2));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().BeNull();
    }

    [Fact]
    public void UnequippingItemStackWhenInventoryHasInsufficientSpaceFillsAvailableSpaceAndLeavesRemainderEquipped()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestPotion1Item>(9));
        _sut.Inventory.PutAt(1, ItemStack.Of<TestPotion1Item>(8));
        for (int i = 2; i < _sut.Inventory.Count; i++)
            _sut.Inventory.PutAt(i, ItemStack.Of<TestHatItem>(1));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion] =
            new EquipmentSlotState { ItemStack = ItemStack.Of<TestPotion1Item>(5) };

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory.Take(2).All(it => it == ItemStack.Of<TestPotion1Item>(10)).Should().BeTrue();
        _sut.Inventory.Skip(2).All(it => it == ItemStack.Of<TestHatItem>(1)).Should().BeTrue();
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().Be(ItemStack.Of<TestPotion1Item>(2));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void UnequippingItemStackToOccupiedOrNonexistentInventorySlotDoesNothing(int inventorySlot)
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestPotion1Item>(1));
        _sut.Inventory.PutAt(1, ItemStack.Of<TestPotion1Item>(10));
        _sut.Inventory.PutAt(2, ItemStack.Of<TestPotion2Item>(1));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion] =
            new EquipmentSlotState { ItemStack = ItemStack.Of<TestPotion1Item>(1) };

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion, inventorySlot));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion1Item>(1));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestPotion1Item>(10));
        _sut.Inventory[2].Should().Be(ItemStack.Of<TestPotion2Item>(1));
        _sut.Inventory[3].Should().BeNull();
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().Be(ItemStack.Of<TestPotion1Item>(1));
    }

    [Fact]
    public void UnequippingItemStackToEmptyInventorySlotMovesItThere()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestPotion1Item>(1));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion] =
            new EquipmentSlotState { ItemStack = ItemStack.Of<TestPotion1Item>(1) };

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion, 1));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion1Item>(1));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestPotion1Item>(1));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().BeNull();
    }

    [Fact]
    public void UnequippingItemStackToInventorySlotWithInsufficientSpaceMovesWhatPossibleAndKeepsRestEquipped()
    {
        _sut.Inventory.PutAt(0, ItemStack.Of<TestPotion1Item>(4));
        _sut.Inventory.PutAt(1, ItemStack.Of<TestPotion1Item>(7));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion] =
            new EquipmentSlotState { ItemStack = ItemStack.Of<TestPotion1Item>(8) };

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion, 1));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion1Item>(4));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestPotion1Item>(10));
        _sut.EquipmentSlotStates[EquipmentSlot.Potion].ItemStack.Should().Be(ItemStack.Of<TestPotion1Item>(5));
    }

    private sealed class Sut : Unit
    {
        public Sut(Guid id, IUnitNode node, IEntityManager entityManager, IServiceProvider serviceProvider) :
            base(id, controllingPlayerId: id, node, entityManager, serviceProvider) { }

        public new void SetAbility<T>(AbilitySlot slot, int level) where T : Ability, new() =>
            base.SetAbility<T>(slot, level);

        public new IDictionary<EquipmentSlot, EquipmentSlotState> EquipmentSlotStates =>
            EquipmentSlotStatesInternal;

        public override IReadOnlyList<EquipmentSlot> EquipmentSlots => [EquipmentSlot.Potion];
    }

    public class SpyAbility : Ability
    {
        public event Action Used = delegate { };
        public event Action ValidatedStrictly = delegate { };
        public event Action ValidatedNonStrictly = delegate { };

        public override Targeting Targeting => Targeting.Anything;

        public override AbilityValidationResult Validate(AbilityContext context, bool strict)
        {
            if (strict) ValidatedStrictly();
            else ValidatedNonStrictly();
            return base.Validate(context, strict);
        }

        public override void TakeEffect(AbilityContext context)
        {
            base.TakeEffect(context);
            Used();
        }
    }

    public class SpyAbilityWithUseTime : SpyAbility
    {
        public override Scalable<double> StaticUseTime => 1;
    }

    public class AbilityWithUseTime : Ability
    {
        public override Targeting Targeting => Targeting.Anything;
        public override Scalable<double> StaticUseTime => 1;
    }

    public sealed class StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff : Status
    {
        public const double BuffValue = 5;

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

        public override void OnDealAttackDamage(StatusContext context, IUnit target, double damage)
        {
            base.OnDealAttackDamage(context, target, damage);
            context.Unit.RemoveStatus(context.Id);
        }
    }

    public sealed class DieOnTickStatus : Status
    {
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;
        public override void Tick(StatusContext context, double delta) => context.Unit.Die();
    }

    private sealed class TestPotion1Item : Item
    {
        public override EquipmentSlot? EquippableTo => EquipmentSlot.Potion;
        public override long MaxStackCount => 10;
    }

    private sealed class TestPotion2Item : Item
    {
        public override EquipmentSlot? EquippableTo => EquipmentSlot.Potion;
        public override long MaxStackCount => 10;
    }

    private sealed class TestHatItem : Item
    {
        public override EquipmentSlot? EquippableTo => Enums.EquipmentSlot.Head;
    }
}
