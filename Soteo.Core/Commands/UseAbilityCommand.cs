using Soteo.Core.Snapshots;

namespace Soteo.Core.Commands;

public sealed record UseAbilityCommand : Command
{
    public required AbilitySlot Slot { get; init; }
    public bool Repeat { get; init; }
    public bool Alt { get; init; }
    public AbilityTargetSnapshot Target { get; init; } = AbilityTargetSnapshot.Nothing;
}
