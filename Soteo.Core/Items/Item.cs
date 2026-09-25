using System.Collections.Immutable;
using Soteo.Core.Abilities;
using Soteo.Core.Enums;

namespace Soteo.Core.Items;

public abstract class Item : SingletonHierarchyBase<Item>
{
    public virtual int StackSize => 1;

    public virtual EquipmentSlot? EquipmentSlot => null;

    /// <summary>
    /// Extra equipment slots that the item occupies in addition to its main slot,
    /// preventing other items from being equipped to them
    /// </summary>
    public virtual IReadOnlyList<EquipmentSlot> SecondaryEquipmentSlots => [];

    public virtual IReadOnlyDictionary<AbilitySlot, Ability> Abilities =>
        ImmutableDictionary<AbilitySlot, Ability>.Empty;

    public virtual string Name =>
        GetType().Name.ReplaceRegex("Item$", "").PascalCaseToCapitalizedText();

    /// <summary>
    /// Path to the ability icon relative to res://Textures/Icons, without an extension
    /// </summary>
    public virtual string IconPath => "Placeholder";
}
