namespace Soteo.Core.Statuses;

/// <param name="Priority">
/// When modifiers contradict each other, the one with the highest priority wins.
/// If priorities are equal, the one with the highest value wins.
/// Set is always prioritized over floor / ceiling, regardless of priority.
/// </param>
public sealed record StatModifier(Stat Stat, StatModifierKind Kind, double Value, double Priority = 0);
