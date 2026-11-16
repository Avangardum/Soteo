using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Soteo.Core.Abilities;
using Soteo.Core.Commands;
using Soteo.Core.Entities;
using Soteo.Core.Entities.Projectiles;
using Soteo.Core.Entities.Units;
using Soteo.Core.Items;
using Soteo.Core.Statuses;
using Soteo.TestUtil;

namespace Soteo.Core.Tests;

public sealed class UnitTests
{
    private readonly TestUnit _sut;
    private readonly IEntityManager _entityManager;
    private readonly IUnitNode _node;
    private readonly IServiceProvider _serviceProvider;

    public UnitTests()
    {
        _entityManager = Substitute.For<IEntityManager>();
        _node = Substitute.For<IUnitNode>();
        _serviceProvider = new ServiceCollection().AddSingleton(_entityManager).BuildServiceProvider();
        var inventory = new PlayerInventory(8);
        var equipmentInventory =
            new PlayerEquipmentInventory(EquipmentSlot.MainHand ,EquipmentSlot.Head, EquipmentSlot.Potion);
        Guid id = Guid.NewGuid();
        _sut = new TestUnit
        (
            id,
            id,
            _node,
            _entityManager,
            inventory,
            equipmentInventory,
            new CrossInventoryHelper(),
            new Services(_serviceProvider)
        );
    }

    [Fact]
    public void UseAbilityCommandForEmptySlotDoesntThrow()
    {
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(0), TargetUnitId = Guid.NewGuid() });
        _sut.Tick(Const.TickInterval);
    }

    [Fact]
    public void AbilityTargetingNonexistentUnitIsNotUsed()
    {
        _sut.AddInnateHotbarAbility<SpyAbility>();
        int useCount = 0;
        Ability.Instance<SpyAbility>().Used += () => useCount++;

        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(0), TargetUnitId = Guid.NewGuid() });
        _sut.Tick(Const.TickInterval);

        useCount.Should().Be(0);
    }

    [Fact]
    public void AbilityTargetingProjectileIsNotUsed()
    {
        _sut.AddInnateHotbarAbility<SpyAbility>();
        int useCount = 0;
        Ability.Instance<SpyAbility>().Used += () => useCount++;
        var projectileId = Guid.NewGuid();
        var projectileAbilityContext = new AbilityContext
        {
            Ability = Ability.Instance<SpyAbility>(),
            User = _sut,
            UserStats = _sut.GetStats().ToDictionary().AsReadOnly(),
            Services = new Services(_serviceProvider)
        };
        var projectile = new Projectile
        (
            projectileId,
            projectileAbilityContext,
            speed: 1,
            target: new ProjectileTarget(_sut),
            new Services(_serviceProvider)
        );
        _entityManager.Entities.Returns(new Dictionary<Guid, Entity> { [projectileId] = projectile });

        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(0), TargetUnitId = projectileId });
        _sut.Tick(Const.TickInterval);

        useCount.Should().Be(0);
    }

    [Fact]
    public void StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuffChangesStatsCorrectly()
    {
        _sut.AddStatus<StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff>
        (
            time: double.PositiveInfinity,
            source: null
        );

        const double buffValue = StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff.BuffValue;
        _sut.GetStat(Stat.MoveSpeed).Should().Be(Stat.MoveSpeed.Default + buffValue);
        _sut.GetStat(Stat.AttackSpeed).Should().Be(Stat.AttackSpeed.Default);

        Ticker.Tick(_sut.Tick).WithDefaultInterval().ForAtLeast(1.5);

        _sut.GetStat(Stat.MoveSpeed).Should().Be(Stat.MoveSpeed.Default);
        _sut.GetStat(Stat.AttackSpeed).Should().Be(Stat.AttackSpeed.Default + buffValue);
    }

    [Fact]
    public void StatusIsTickedEveryTick()
    {
        int statusTickCount = 0;
        RefreshableSpyStatus.Ticked += () => statusTickCount++;

        _sut.AddStatus<RefreshableSpyStatus>(time: 10, source: null);
        statusTickCount.Should().Be(0);

        _sut.Tick(Const.TickInterval);
        statusTickCount.Should().Be(1);

        _sut.Tick(Const.TickInterval);
        statusTickCount.Should().Be(2);

        _sut.Tick(Const.TickInterval);
        statusTickCount.Should().Be(3);
    }

    [Fact]
    public void StatusRemovedOnDealAttackDamageIsRemovedAccordingly()
    {
        _sut.AddStatus<StatusRemovedOnDealAttackDamage>(time: 10, source: null);
        _sut.GetStatusContexts().Should().NotBeEmpty();
        _sut.DealAttackDamageTo(_sut, Ability.Instance<MeleeAttackAbility>());
        _sut.GetStatusContexts().Should().BeEmpty();
    }

    [Fact]
    public void DieOnTickStatusKillsItsUnitWhenTickedAndPreventsOnlyStatusesAfterItFromTicking()
    {
        bool wasStackableSpyStatusTicked = false;
        StackableSpyStatus.Ticked += () => wasStackableSpyStatusTicked = true;
        bool wasRefreshableSpyStatusTicked = false;
        RefreshableSpyStatus.Ticked += () => wasRefreshableSpyStatusTicked = true;

        _sut.AddStatus<StackableSpyStatus>(double.PositiveInfinity, null);
        _sut.AddStatus<DieOnTickStatus>(double.PositiveInfinity, null);
        _sut.AddStatus<RefreshableSpyStatus>(double.PositiveInfinity, null);
        _sut.Tick(Const.TickInterval);

        _sut.IsDead.Should().BeTrue();
        wasStackableSpyStatusTicked.Should().BeTrue();
        wasRefreshableSpyStatusTicked.Should().BeFalse();
    }

    [Fact]
    public void UsingAbilityValidatesItInitiallyThenContinuouslyWhileUsing()
    {
        var slot = AbilitySlot.Hotbar(_sut.GetInnateHotbarAbilities().Count);
        _sut.AddInnateHotbarAbility<SpyAbilityWithUseTime>();
        int initialValidationCount = 0;
        int continuousValidationCount = 0;
        Ability.Instance<SpyAbilityWithUseTime>().ValidatedInitially += () => initialValidationCount++;
        Ability.Instance<SpyAbilityWithUseTime>().ValidatedContinuously += () => continuousValidationCount++;

        _sut.SetCommand(new UseAbilityCommand { Slot = slot });
        _sut.Tick(Const.TickInterval);

        initialValidationCount.Should().Be(1);
        continuousValidationCount.Should().Be(0);

        _sut.Tick(Const.TickInterval);

        initialValidationCount.Should().Be(1);
        continuousValidationCount.Should().Be(1);

        _sut.Tick(Const.TickInterval);

        initialValidationCount.Should().Be(1);
        continuousValidationCount.Should().Be(2);
    }

    [Fact]
    public void UsingAbilityWhileAnotherAbilityIsBeingUsedValidatesItStrictlyInitiallyThenNonStrictlyWhileUsing()
    {
        _sut.AddInnateHotbarAbility<AbilityWithUseTime>();
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(0) });
        _sut.Tick(Const.TickInterval);

        UsingAbilityValidatesItInitiallyThenContinuouslyWhileUsing();
    }

    [Fact]
    public void MovingItemStackFromInventorySlotToAnotherInventorySlotMovesItThereAndMovesExistingStackToSourceSlot()
    {
        _sut.Inventory.Put<TestItem1>(0, 1);
        _sut.Inventory.Put<TestItem2>(1, 1);

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
        _sut.Inventory.Put<TestItem1>(0, 1);
        _sut.Inventory.Put<TestItem2>(1, 1);

        _sut.SetCommand(new MoveItemStackCommand { FromIndex = -228, ToIndex = 1 });
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestItem1>(1));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestItem2>(1));
    }

    [Fact]
    public void MovingItemStackToInvalidInventorySlotDoesNothing()
    {
        _sut.Inventory.Put<TestItem1>(0, 1);
        _sut.Inventory.Put<TestItem2>(1, 1);

        _sut.SetCommand(new MoveItemStackCommand { FromIndex = 0, ToIndex = int.MaxValue });
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestItem1>(1));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestItem2>(1));
    }

    [Fact]
    public void EquippingUnequippableItemStackDoesNothing()
    {
        _sut.Inventory.Put<TestItem1>(0, 1);

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestItem1>(1));
        _sut.EquipmentInventory.Values.WhereNotNull().Should().BeEmpty();
    }

    [Fact]
    public void EquippingItemStackIntoOccupiedSlotDoesNothing()
    {
        _sut.Inventory.Put<TestPotion2>(0, 1);
        _sut.EquipmentInventory.Put<TestPotion1>(EquipmentSlot.Potion, 1);

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion2>(1));
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().Be(ItemStack.Of<TestPotion1>(1));
    }

    [Fact]
    public void EquippingItemStackIntoBlockedSlotDoesNothing()
    {
        _sut.Inventory.Put<TestPotion2>(0, 1);
        _sut.EquipmentInventory.Put<HatThatMakesYouHatePotions>(EquipmentSlot.Head, 1);

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion2>(1));
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().BeNull();
    }

    [Fact]
    public void EquippingItemStackWhenSlotItBlocksIsOccupiedDoesNothing()
    {
        _sut.Inventory.Put<HatThatMakesYouHatePotions>(0, 1);
        _sut.EquipmentInventory.Put<TestPotion1>(EquipmentSlot.Potion, 1);

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<HatThatMakesYouHatePotions>(1));
        _sut.EquipmentInventory[EquipmentSlot.Head].Should().BeNull();
    }

    [Fact]
    public void EquippingItemStackWhenSlotItBlocksIsBlockedDoesNothing()
    {
        _sut.Inventory.Put<HatThatMakesYouHatePotions>(0, 1);
        _sut.EquipmentInventory.Put<SwordThatMakesYouHatePotions>(EquipmentSlot.MainHand, 1);

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<HatThatMakesYouHatePotions>(1));
        _sut.EquipmentInventory[EquipmentSlot.Head].Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void EquippingFromEmptyOrNonexistentInventorySlotDoesNothing(int inventorySlot)
    {
        _sut.SetCommand(new EquipCommand(inventorySlot));
        _sut.Tick(Const.TickInterval);

        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().BeNull();
    }

    [Fact]
    public void EquippingToNonexistentEquipmentSlotDoesNothing()
    {
        _sut.Inventory.Put<TestBoots>(0, 1);

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestBoots>(1));
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().BeNull();
        _sut.EquipmentInventory.Should().NotContainKey(EquipmentSlot.Feet);
    }

    [Fact]
    public void EquippingItemStackToEmptyEquipmentSlotMovesItToThatSlot()
    {
        _sut.Inventory.Put<TestPotion1>(0, 1);

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().BeNull();
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().Be(ItemStack.Of<TestPotion1>(1));
    }

    [Fact]
    public void EquippingItemStackToEquipmentSlotOccupiedBySameItemEquipsWhatIsPossibleAndLeavesRestInInventory()
    {
        _sut.Inventory.Put<TestPotion1>(0, 5);
        _sut.EquipmentInventory.Put<TestPotion1>(EquipmentSlot.Potion, 8);

        _sut.SetCommand(new EquipCommand(0));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion1>(3));
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().Be(ItemStack.Of<TestPotion1>(10));
    }

    [Theory]
    [InlineData(EquipmentSlot.Head)]
    [InlineData(EquipmentSlot.Potion)]
    public void UnequippingItemFromNonexistentOrEmptyEquipmentSlotDoesNothing(EquipmentSlot slot)
    {
        _sut.SetCommand(new UnequipCommand(slot));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory.WhereNotNull().Should().BeEmpty();
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().BeNull();
    }

    [Fact]
    public void UnequippingItemWhenInventoryHasNoSpaceDoesNothing()
    {
        for (int i = 0; i < _sut.Inventory.Count; i++)
            _sut.Inventory.Put<TestBoots>(i, 1);
        _sut.EquipmentInventory.Put<TestPotion1>(EquipmentSlot.Potion, 1);

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory.All(it => it == ItemStack.Of<TestBoots>(1)).Should().BeTrue();
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().Be(ItemStack.Of<TestPotion1>(1));
    }

    [Fact]
    public void UnequippingItemFillsAvailableSpaceMergingStacksIfPossible()
    {
        _sut.Inventory.Put<TestPotion1>(0, 9);
        _sut.Inventory.Put<TestPotion1>(1, 8);
        _sut.EquipmentInventory.Put<TestPotion1>(EquipmentSlot.Potion, 5);

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion1>(10));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestPotion1>(10));
        _sut.Inventory[2].Should().Be(ItemStack.Of<TestPotion1>(2));
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().BeNull();
    }

    [Fact]
    public void UnequippingItemStackWhenInventoryHasInsufficientSpaceFillsAvailableSpaceAndLeavesRemainderEquipped()
    {
        _sut.Inventory.Put<TestPotion1>(0, 9);
        _sut.Inventory.Put<TestPotion1>(1, 8);
        for (int i = 2; i < _sut.Inventory.Count; i++)
            _sut.Inventory.Put<TestBoots>(i, 1);
        _sut.EquipmentInventory.Put<TestPotion1>(EquipmentSlot.Potion, 5);

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory.Take(2).All(it => it == ItemStack.Of<TestPotion1>(10)).Should().BeTrue();
        _sut.Inventory.Skip(2).All(it => it == ItemStack.Of<TestBoots>(1)).Should().BeTrue();
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().Be(ItemStack.Of<TestPotion1>(2));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void UnequippingItemStackToOccupiedOrNonexistentInventorySlotDoesNothing(int inventorySlot)
    {
        _sut.Inventory.Put<TestPotion1>(0, 1);
        _sut.Inventory.Put<TestPotion1>(1, 10);
        _sut.Inventory.Put<TestPotion2>(2, 1);
        _sut.EquipmentInventory.Put<TestPotion1>(EquipmentSlot.Potion, 1);

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion, inventorySlot));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion1>(1));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestPotion1>(10));
        _sut.Inventory[2].Should().Be(ItemStack.Of<TestPotion2>(1));
        _sut.Inventory[3].Should().BeNull();
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().Be(ItemStack.Of<TestPotion1>(1));
    }

    [Fact]
    public void UnequippingItemStackToEmptyInventorySlotMovesItThere()
    {
        _sut.Inventory.Put<TestPotion1>(0, 1);
        _sut.EquipmentInventory.Put<TestPotion1>(EquipmentSlot.Potion, 1);

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion, 1));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion1>(1));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestPotion1>(1));
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().BeNull();
    }

    [Fact]
    public void UnequippingItemStackToInventorySlotWithInsufficientSpaceMovesWhatPossibleAndKeepsRestEquipped()
    {
        _sut.Inventory.Put<TestPotion1>(0, 4);
        _sut.Inventory.Put<TestPotion1>(1, 7);
        _sut.EquipmentInventory.Put<TestPotion1>(EquipmentSlot.Potion, 8);

        _sut.SetCommand(new UnequipCommand(EquipmentSlot.Potion, 1));
        _sut.Tick(Const.TickInterval);

        _sut.Inventory[0].Should().Be(ItemStack.Of<TestPotion1>(4));
        _sut.Inventory[1].Should().Be(ItemStack.Of<TestPotion1>(10));
        _sut.EquipmentInventory[EquipmentSlot.Potion].Should().Be(ItemStack.Of<TestPotion1>(5));
    }

    [Fact]
    public void UnitWithoutInnateAbilitiesOrEquippedItemsHasNoAbilityStates()
    {
        _sut.GetAbilitySlotStates().Should().BeEmpty();
    }

    [Fact]
    public void AddingInnateAbilitiesAndEquipmentWithAbilitiesAddsAbilitySlotStates()
    {
        _sut.AddInnateHotbarAbility<SpyAbility>();
        _sut.EquipmentInventory.Put<SwordWith1SCdAttackAnd10SCdHotbarAbility>(EquipmentSlot.MainHand, 1);

        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Ability
            .Should().Be(Ability.Instance<AbilityWith1SCooldown>());
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Ability
            .Should().Be(Ability.Instance<SpyAbility>());
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(1)).Required.Ability
            .Should().Be(Ability.Instance<AbilityWith10SCooldown>());
    }

    [Fact]
    public void AddingInnateAbilityMovesExistingItemAbilityStateToNextSlot()
    {
        _sut.EquipmentInventory
            .Put<SwordWith1SCdAttackAnd10SCdHotbarAbility>(EquipmentSlot.MainHand, 1);
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(0) } );
        _sut.Tick(Const.TickInterval);
        double cooldown = _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Cooldown;
        cooldown.Should().BePositive();

        _sut.AddInnateHotbarAbility<SpyAbility>();

        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Ability
            .Should().Be(Ability.Instance<SpyAbility>());
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Cooldown
            .Should().Be(0);
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(1)).Required.Ability
            .Should().Be(Ability.Instance<AbilityWith10SCooldown>());
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(1)).Required.Cooldown
            .Should().Be(cooldown);
    }

    [Fact]
    public void RemovingInnateAbilityMovesExistingItemAbilityStateToPrevSlot()
    {
        _sut.AddInnateHotbarAbility<SpyAbility>();
        _sut.EquipmentInventory
            .Put<SwordWith1SCdAttackAnd10SCdHotbarAbility>(EquipmentSlot.MainHand, 1);
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(1) } );
        _sut.Tick(Const.TickInterval);
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(1)).Required.Ability.Should().Be(Ability.Instance<AbilityWith10SCooldown>());
        double cooldown = _sut.GetAbilitySlotState(AbilitySlot.Hotbar(1)).Required.Cooldown;
        cooldown.Should().BePositive();

        _sut.RemoveInnateHotbarAbility<SpyAbility>();

        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Ability.Should().Be(Ability.Instance<AbilityWith10SCooldown>());
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Cooldown.Should().Be(cooldown);
        _sut.GetAbilitySlotStates().Should().NotContainKey(AbilitySlot.Hotbar(1));
    }

    [Fact]
    public void AddedItemAttackReplacesExistingInnateAttackWithCooldownCarryingOver()
    {
        _sut.SetInnateSpecialAbility<AbilityWith10SCooldown>(AbilitySlot.Attack);
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Attack });
        _sut.Tick(Const.TickInterval);
        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Ability
            .Should().Be(Ability.Instance<AbilityWith10SCooldown>());
        double cooldown = _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Cooldown;
        cooldown.Should().BePositive();

        _sut.EquipmentInventory
            .Put<SwordWith1SCdAttackAnd10SCdHotbarAbility>(EquipmentSlot.MainHand, 1);

        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Ability
            .Should().Be(Ability.Instance<AbilityWith1SCooldown>());
        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Cooldown.Should().Be(cooldown);
    }

    [Fact]
    public void RemovedItemAttackIsReplacedByExistingInnateAttackWithCooldownCarryingOver()
    {
        _sut.SetInnateSpecialAbility<AbilityWith10SCooldown>(AbilitySlot.Attack);
        _sut.EquipmentInventory
            .Put<SwordWith1SCdAttackAnd10SCdHotbarAbility>(EquipmentSlot.MainHand, 1);
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Attack });
        _sut.Tick(Const.TickInterval);
        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Ability
            .Should().Be(Ability.Instance<AbilityWith1SCooldown>());
        double cooldown = _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Cooldown;
        cooldown.Should().BePositive();

        _sut.EquipmentInventory.Remove(EquipmentSlot.MainHand);

        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Ability
            .Should().Be(Ability.Instance<AbilityWith10SCooldown>());
        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Cooldown.Should().Be(cooldown);
    }

    [Fact]
    public void AddedInnateAttackDoesNotReplaceExistingItemAttack()
    {
        _sut.EquipmentInventory
            .Put<SwordWith1SCdAttackAnd10SCdHotbarAbility>(EquipmentSlot.MainHand, 1);
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Attack });
        _sut.Tick(Const.TickInterval);
        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Ability
            .Should().Be(Ability.Instance<AbilityWith1SCooldown>());
        double cooldown = _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Cooldown;
        cooldown.Should().BePositive();

        _sut.SetInnateSpecialAbility<AbilityWith10SCooldown>(AbilitySlot.Attack);

        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Ability
            .Should().Be(Ability.Instance<AbilityWith1SCooldown>());
        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Cooldown.Should().Be(cooldown);
    }

    [Fact]
    public void AddingInnateAbilityDoesNotChangeExistingSlotCooldowns()
    {
        _sut.SetInnateSpecialAbility<AbilityWith1SCooldown>(AbilitySlot.Attack);
        _sut.AddInnateHotbarAbility<AbilityWith10SCooldown>();
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Attack } );
        _sut.Tick(Const.TickInterval);
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(0) } );
        _sut.Tick(Const.TickInterval);
        double attackCd = _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Cooldown;
        double hotbarCd = _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Cooldown;
        attackCd.Should().BePositive();
        hotbarCd.Should().BePositive();

        _sut.AddInnateHotbarAbility<SpyAbility>();

        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Cooldown.Should().Be(attackCd);
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Cooldown.Should().Be(hotbarCd);
    }

    [Fact]
    public void RemovingInnateAbilityDoesNotChangeExistingSlotCooldowns()
    {
        _sut.SetInnateSpecialAbility<AbilityWith1SCooldown>(AbilitySlot.Attack);
        _sut.AddInnateHotbarAbility<AbilityWith10SCooldown>();
        _sut.AddInnateHotbarAbility<SpyAbility>();
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Attack } );
        _sut.Tick(Const.TickInterval);
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(0) } );
        _sut.Tick(Const.TickInterval);
        double attackCd = _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Cooldown;
        double hotbarCd = _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Cooldown;
        attackCd.Should().BePositive();
        hotbarCd.Should().BePositive();

        _sut.RemoveInnateHotbarAbility<SpyAbility>();

        _sut.GetAbilitySlotState(AbilitySlot.Attack).Required.Cooldown.Should().Be(attackCd);
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Cooldown.Should().Be(hotbarCd);
    }

    [Theory]
    [MemberData(nameof(AddingAbilityResetsAbilityUseProgressAndCommandsData))]
    public void AddingAbilityResetsAbilityUseProgressAndCommands(Action<TestUnit> addAbility)
    {
        _sut.AddInnateHotbarAbility<AbilityWithUseTime>();
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(0) });
        _sut.Tick(Const.TickInterval);
        _sut.AbilityUseProgress.Required.Slot.Should().Be(AbilitySlot.Hotbar(0));
        _sut.GetCommands().Should().NotBeEmpty();

        addAbility(_sut);

        _sut.AbilityUseProgress.Should().BeNull();
        _sut.GetCommands().Should().BeEmpty();
    }

    public static List<object[]> AddingAbilityResetsAbilityUseProgressAndCommandsData =
    [
        [void (TestUnit sut) => sut.AddInnateHotbarAbility<SpyAbility>()],
        [void (TestUnit sut) => sut.SetInnateSpecialAbility<SpyAbility>(AbilitySlot.Attack)],
        [
            void (TestUnit sut) => sut.EquipmentInventory.Put
            (
                EquipmentSlot.MainHand,
                ItemStack.Of<SwordWith1SCdAttackAnd10SCdHotbarAbility>(1)
            )
        ],
    ];

    [Fact]
    public void AddingFirstInnateHotbarAbilityThatIsSameAsExistingItemHotbarAbilityMovesThatAbilityStateToFirstSlot()
    {
        _sut.EquipmentInventory.Put<SwordWith1SAnd10SCdHotbarAbilities>(EquipmentSlot.MainHand, 1);
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(1) } );
        _sut.Tick(Const.TickInterval);
        double cooldown = _sut.GetAbilitySlotState(AbilitySlot.Hotbar(1)).Required.Cooldown;
        cooldown.Should().BePositive();

        _sut.AddInnateHotbarAbility<AbilityWith10SCooldown>();

        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Ability.Should().Be(Ability.Instance<AbilityWith10SCooldown>());
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Cooldown.Should().Be(cooldown);
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(1)).Required.Ability.Should().Be(Ability.Instance<AbilityWith1SCooldown>());
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(1)).Required.Cooldown.Should().Be(0);
        _sut.GetAbilitySlotStates().Should().NotContainKey(AbilitySlot.Hotbar(2));
    }

    [Fact]
    public void RemovingFirstInnateHotbarAbilityThatIsSameAsExistingItemHotbarAbilityMovesThatAbilityStateAfterInnates()
    {
        _sut.AddInnateHotbarAbility<AbilityWith10SCooldown>();
        _sut.AddInnateHotbarAbility<SpyAbility>();
        _sut.AddInnateHotbarAbility<SpyAbilityWithUseTime>();
        _sut.EquipmentInventory.Put<SwordWith1SCdAttackAnd10SCdHotbarAbility>(EquipmentSlot.MainHand, 1);
        _sut.SetCommand(new UseAbilityCommand { Slot = AbilitySlot.Hotbar(0) } );
        _sut.Tick(Const.TickInterval);
        double cooldown = _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Cooldown;
        cooldown.Should().BePositive();

        _sut.RemoveInnateHotbarAbility<AbilityWith10SCooldown>();

        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Ability
            .Should().Be(Ability.Instance<SpyAbility>());
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(0)).Required.Cooldown
            .Should().Be(0);
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(1)).Required.Ability
            .Should().Be(Ability.Instance<SpyAbilityWithUseTime>());
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(1)).Required.Cooldown
            .Should().Be(0);
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(2)).Required.Ability
            .Should().Be(Ability.Instance<AbilityWith10SCooldown>());
        _sut.GetAbilitySlotState(AbilitySlot.Hotbar(2)).Required.Cooldown
            .Should().Be(cooldown);
        _sut.GetAbilitySlotStates().Should().NotContainKey(AbilitySlot.Hotbar(3));
    }

    [Fact]
    public void AddingInnateAbilityWithPassiveStatusAddsThatStatus()
    {
        _sut.AddInnateHotbarAbility<TestPassiveAbility>();
        _sut.GetStatusContexts().Values.Single().Status.Should().BeOfType<TestAbilityPassiveStatus>();
    }

    [Fact]
    public void RemovingInnateAbilityWithPassiveStatusRemovesThatStatus()
    {
        _sut.AddInnateHotbarAbility<TestPassiveAbility>();
        _sut.RemoveInnateHotbarAbility<TestPassiveAbility>();
        _sut.GetStatusContexts().Should().BeEmpty();
    }

    [Fact]
    public void AddingAttackWithPassiveStatusAddsThatStatus()
    {
        _sut.SetInnateSpecialAbility<TestPassiveAbility>(AbilitySlot.Attack);
        _sut.GetStatusContexts().Values.Single().Status.Should().BeOfType<TestAbilityPassiveStatus>();
    }

    [Fact]
    public void RemovingAttackWithPassiveStatusRemovesThatStatus()
    {
        _sut.SetInnateSpecialAbility<TestPassiveAbility>(AbilitySlot.Attack);
        _sut.RemoveInnateSpecialAbility(AbilitySlot.Attack);
        _sut.GetStatusContexts().Should().BeEmpty();
    }

    [Fact]
    public void AddingAbilityPassiveStatusManuallyThrows()
    {
        FluentActions.Invoking(() => _sut.AddStatus<TestAbilityPassiveStatus>(double.PositiveInfinity, null))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemovingAbilityPassiveStatusManuallyThrows()
    {
        _sut.AddInnateHotbarAbility<TestPassiveAbility>();

        FluentActions.Invoking(() => _sut.RemoveAllStatuses<TestAbilityPassiveStatus>())
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddingThenRemovingStatusThatSetsMaxManaTo0SetsCurrentManaTo0()
    {
        _sut.AddStatus<StatusThatSetsMaxManaTo0>(10, null);
        _sut.RemoveAllStatuses<StatusThatSetsMaxManaTo0>();

        _sut.GetStat(Stat.CurrentMana).Should().Be(0);
    }

    [Fact]
    public void StatusThatRemovesAllStatusesOnTickRemovesAllStatusesAndPreventsOnlyStatusesAfterItFromTicking()
    {
        bool wasStackableSpyStatusTicked = false;
        StackableSpyStatus.Ticked += () => wasStackableSpyStatusTicked = true;
        bool wasRefreshableSpyStatusTicked = false;
        RefreshableSpyStatus.Ticked += () => wasRefreshableSpyStatusTicked = true;

        _sut.AddStatus<StackableSpyStatus>(double.PositiveInfinity, null);
        _sut.AddStatus<StatusThatRemovesAllStatusesOnTick>(double.PositiveInfinity, null);
        _sut.AddStatus<RefreshableSpyStatus>(double.PositiveInfinity, null);
        _sut.Tick(Const.TickInterval);

        _sut.GetStatusContexts().Should().BeEmpty();
        wasStackableSpyStatusTicked.Should().BeTrue();
        wasRefreshableSpyStatusTicked.Should().BeFalse();
    }

    private class SpyAbility : Ability
    {
        public event Action Used = delegate { };
        public event Action ValidatedInitially = delegate { };
        public event Action ValidatedContinuously = delegate { };

        public override Targeting Targeting => Targeting.Anything;

        protected override AbilityValidationResult ValidateCore(AbilityContext context, AbilityValidationMode mode)
        {
            switch (mode)
            {
                case AbilityValidationMode.Initial:
                    ValidatedInitially();
                    break;
                case AbilityValidationMode.Continuous:
                    ValidatedContinuously();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return AbilityValidationResult.Ok;
        }

        protected override void TakeEffectCore(AbilityContext context)
        {
            Used();
        }
    }

    private class SpyAbilityWithUseTime : SpyAbility
    {
        public override double StaticUseTime => 1;
    }

    private class AbilityWithUseTime : Ability
    {
        public override Targeting Targeting => Targeting.Anything;
        public override double StaticUseTime => 1;
    }

    private sealed class StatusAlternatingBetweenMoveSpeedAndAttackSpeedBuff : Status
    {
        public const double BuffValue = 5;

        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;

        public override IReadOnlyList<StatModifier> StatModifiers(StatusContext context)
        {
            Stat stat = Maths.FloorToInt(context.ElapsedTime) % 2 == 0 ? Stat.MoveSpeed : Stat.AttackSpeed;
            return [new StatModifier(stat, StatModifierKind.Add, BuffValue)];
        }
    }

    private sealed class StackableSpyStatus : Status
    {
        public static event Action Ticked = delegate { };

        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Stack;

        public override void Tick(StatusContext context, double delta)
        {
            base.Tick(context, delta);
            Ticked();
        }
    }

    private sealed class RefreshableSpyStatus : Status
    {
        public static event Action Ticked = delegate { };

        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Refresh;

        public override void Tick(StatusContext context, double delta)
        {
            base.Tick(context, delta);
            Ticked();
        }
    }

    private sealed class StatusRemovedOnDealAttackDamage : Status
    {
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;

        public override void OnDealAttackDamage(StatusContext context, Unit target, double damage)
        {
            base.OnDealAttackDamage(context, target, damage);
            context.Unit.RemoveStatus(context.Id);
        }
    }

    private sealed class DieOnTickStatus : Status
    {
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;
        public override void Tick(StatusContext context, double delta) => context.Unit.Die();
    }

    private sealed class TestPotion1 : Item
    {
        public override EquipmentSlot? EquippableTo => EquipmentSlot.Potion;
        public override long MaxStackCount => 10;
    }

    private sealed class TestPotion2 : Item
    {
        public override EquipmentSlot? EquippableTo => EquipmentSlot.Potion;
        public override long MaxStackCount => 10;
    }

    private sealed class TestBoots : Item
    {
        public override EquipmentSlot? EquippableTo => EquipmentSlot.Feet;
    }

    private sealed class HatThatMakesYouHatePotions : Item
    {
        public override EquipmentSlot? EquippableTo => EquipmentSlot.Head;
        public override IReadOnlyList<EquipmentSlot> BlocksEquipmentSlots => [EquipmentSlot.Potion];
    }

    private sealed class SwordThatMakesYouHatePotions : Item
    {
        public override EquipmentSlot? EquippableTo => EquipmentSlot.MainHand;
        public override IReadOnlyList<EquipmentSlot> BlocksEquipmentSlots => [EquipmentSlot.Potion];
    }

    private sealed class AbilityWith10SCooldown : Ability
    {
        public override Targeting Targeting => Targeting.Anything;
        public override double StaticCooldown => 10;
    }

    private sealed class AbilityWith1SCooldown : Ability
    {
        public override Targeting Targeting => Targeting.Anything;
        public override double StaticCooldown => 1;
    }

    private sealed class SwordWith1SCdAttackAnd10SCdHotbarAbility : Item
    {
        public override EquipmentSlot? EquippableTo => EquipmentSlot.MainHand;
        public override IReadOnlyList<Ability> HotbarAbilities => [Ability.Instance<AbilityWith10SCooldown>()];

        public override IReadOnlyDictionary<SpecialAbilitySlot, Ability> SpecialAbilities
        {
            get
            {
                return new Dictionary<SpecialAbilitySlot, Ability>
                {
                    [AbilitySlot.Attack] = Ability.Instance<AbilityWith1SCooldown>()
                };
            }
        }
    }

    private sealed class SwordWith1SAnd10SCdHotbarAbilities : Item
    {
        public override EquipmentSlot? EquippableTo => EquipmentSlot.MainHand;

        public override IReadOnlyList<Ability> HotbarAbilities =>
        [
            Ability.Instance<AbilityWith1SCooldown>(),
            Ability.Instance<AbilityWith10SCooldown>()
        ];
    }

    private sealed class TestPassiveAbility : Ability
    {
        public override Targeting Targeting => Targeting.Passive;
        public override AbilityPassiveStatus PassiveStatus => Status.Instance<TestAbilityPassiveStatus>();
    }

    private sealed class TestAbilityPassiveStatus : AbilityPassiveStatus;

    private sealed class StatusThatSetsMaxManaTo0 : Status
    {
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Throw;

        public override IReadOnlyList<StatModifier> StatModifiers(StatusContext context) =>
            [new(Stat.MaxMana, StatModifierKind.Set, 0)];
    }

    private sealed class StatusThatRemovesAllStatusesOnTick : Status
    {
        public override DuplicateStatusResolution DuplicateResolution => DuplicateStatusResolution.Stack;

        public override void Tick(StatusContext context, double delta)
        {
            foreach (Guid id in context.Unit.GetStatusContexts().Keys)
                context.Unit.RemoveStatus(id);
        }
    }
}
