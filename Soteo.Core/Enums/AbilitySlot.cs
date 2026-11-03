using Soteo.Core.Attributes;

namespace Soteo.Core.Enums;

[AllowAnonymousEnumValues]
public enum AbilitySlot
{
    // Special slots, keep synchronized with SpecialAbilitySlot
    Attack,
    Recall,

    // Hotbar slots: both innate and item abilities go here
    Hotbar0,
    Hotbar1,
    Hotbar2,
    Hotbar3,
    Hotbar4,
    Hotbar5,
    Hotbar6,
    // Further hotbar slots are also valid
    HotbarLast = Hotbar6 // todo remove, show unlimited number of abilities and statuses in the HUD
}
