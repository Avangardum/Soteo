using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Dto.Snapshots;

public sealed record UnitPuppetSnapshotDelta : EntitySnapshotDelta
{
    public Delta<bool> IsMoving { get; init; }
    public DictionaryDelta<Stat, double> Stats { get; init; } = new();
    public DictionaryDelta<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; init; } = new();
    public Delta<AbilityUseProgress?> AbilityUseProgress { get; init; }
    public DictionaryDelta<Guid, PuppetStatusContext> Statuses { get; init; } = new();

    public override bool HasChanged
    {
        get
        {
            return base.HasChanged || IsMoving.HasChanged || Stats.HasChanged || AbilitySlotStates.HasChanged ||
                AbilityUseProgress.HasChanged || Statuses.HasChanged;
        }
    }
}