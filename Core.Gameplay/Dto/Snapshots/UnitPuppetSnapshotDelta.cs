using Soteo.Core.Gameplay.Enums;

namespace Soteo.Core.Gameplay.Dto.Snapshots;

public sealed record UnitPuppetSnapshotDelta : EntitySnapshotDelta
{
    public required Delta<bool> IsDead { get; init; }
    public required Delta<bool> IsMoving { get; init; }
    public required DictionaryDelta<Stat, double> Stats { get; init; }
    public required DictionaryDelta<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; init; }
    public required Delta<AbilityUseProgress?> AbilityUseProgress { get; init; }
    public required DictionaryDelta<Guid, PuppetStatusContext> Statuses { get; init; }

    public override bool HasChanged
    {
        get
        {
            return base.HasChanged || IsDead.HasChanged || IsMoving.HasChanged || Stats.HasChanged ||
                AbilitySlotStates.HasChanged || AbilityUseProgress.HasChanged || Statuses.HasChanged;
        }
    }
}
