using Soteo.Gameplay.Enums;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Dto;

/// <param name="Priority">
/// When modifiers contradict each other, the one with the highest priority wins.
/// If priorities are equal, the one with the highest value wins.
/// Set is always prioritized over floor / ceiling, regardless of priority.
/// </param>
public sealed record StatModifier(Stat Stat, StatModifierKind Kind, float Value, float Priority = 0);