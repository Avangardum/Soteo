using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Dto.Snapshots;

public sealed record UnitPuppetSnapshotDelta : EntitySnapshotDelta
{
    public Delta<bool> IsMoving { get; init; }
    
    public DictionaryDelta<Stat, double> Stats { get; init; } = new();
    
    public DictionaryDelta<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; init; } = new();
    
    public Delta<AbilityUseProgress?> AbilityUseProgress { get; init; }
    
    public DictionaryDelta<Guid, PuppetStatusContext> Statuses { get; init; } = new();
    
    public static UnitPuppetSnapshotDelta Between(UnitPuppetSnapshot from, UnitPuppetSnapshot to)
    {
        if (from.Id != to.Id) throw new InvalidOperationException();
        return new UnitPuppetSnapshotDelta
        {
            Id = to.Id,
            Position = Delta.Between(from.Position, to.Position),
            Azimuth = Delta.Between(from.Azimuth, to.Azimuth),
            IsMoving = Delta.Between(from.IsMoving, to.IsMoving),
            Stats = DictionaryDelta.Between(from.Stats, to.Stats),
            AbilitySlotStates = DictionaryDelta.Between(from.AbilitySlotStates, to.AbilitySlotStates),
            AbilityUseProgress = Delta.Between(from.AbilityUseProgress, to.AbilityUseProgress),
            Statuses = DictionaryDelta.Between(from.Statuses, to.Statuses)
        };
    }
}