using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Soteo.Core.Abilities;
using Soteo.Core.Commands;
using Soteo.Core.Entities;
using Soteo.Core.Entities.Units;

namespace Soteo.Core.Tests;

public sealed class TestUnit : Unit
{
    public TestUnit
    (
        Guid? id = null,
        Guid? controllingPlayerId = null,
        IUnitNode? node = null,
        IEntityManager? entityManager = null,
        IInventory? inventory = null,
        IEquipmentInventory? equipmentInventory = null,
        ICrossInventoryHelper? crossInventoryHelper = null,
        Services? services = null
    ) : base
    (
        id ?? Guid.NewGuid(),
        controllingPlayerId ?? Guid.NewGuid(),
        node ?? Substitute.For<IUnitNode>(),
        entityManager ?? Substitute.For<IEntityManager>(),
        inventory ?? new Inventory(0),
        equipmentInventory ?? new EquipmentInventory(0),
        crossInventoryHelper ?? new CrossInventoryHelper(),
        services ?? new Services(new ServiceCollection().BuildServiceProvider())
    ) { }

    public new IReadOnlyDictionary<SpecialAbilitySlot, Ability> GetInnateSpecialAbilities() =>
        base.GetInnateSpecialAbilities();

    public new IReadOnlyList<Ability> GetInnateHotbarAbilities() => base.GetInnateHotbarAbilities();

    public new IReadOnlyList<ICommand> GetCommands() => base.GetCommands();

    public new void AddInnateHotbarAbility<T>() where T : Ability, new() => base.AddInnateHotbarAbility<T>();

    public new void AddInnateHotbarAbility(Ability ability) => base.AddInnateHotbarAbility(ability);

    public new void RemoveInnateHotbarAbility<T>() where T : Ability, new() => base.RemoveInnateHotbarAbility<T>();

    public new void RemoveInnateHotbarAbility(Ability ability) => base.RemoveInnateHotbarAbility(ability);

    public new void SetInnateSpecialAbility<T>(SpecialAbilitySlot slot) where T : Ability, new() =>
        base.SetInnateSpecialAbility<T>(slot);

    public new void SetInnateSpecialAbility(SpecialAbilitySlot slot, Ability ability) =>
        base.SetInnateSpecialAbility(slot, ability);

    public new void RemoveInnateSpecialAbility(SpecialAbilitySlot slot) => base.RemoveInnateSpecialAbility(slot);
}
