using Soteo.Core.Deltas;

namespace Soteo.Core.Snapshots;

public sealed record UnitSnapshot : EntitySnapshot<UnitSnapshot>
{
    public required Type Type { get; init; }
    public required bool IsDead { get; init; }
    public required bool IsMoving { get; init; }
    public required IReadOnlyDictionary<Stat, double> Stats { get; init; }
    public required IReadOnlyDictionary<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; init; }
    public required AbilityUseProgressSnapshot? AbilityUseProgress { get; init; }
    public required IReadOnlyDictionary<Guid, StatusContextSnapshot> StatusesContexts { get; init; }
    public required Guid? ControllingPlayerId { get; init; }
    public required InventorySnapshot InventorySnapshot { get; init; }
    public required EquipmentInventorySnapshot EquipmentInventory { get; init; }
    public required IReadOnlyList<Ability> InnateHotbarAbilities { get; init; }
    public required IReadOnlyDictionary<SpecialAbilitySlot, Ability> InnateSpecialAbilities { get; init; }
    public Faction Faction => Faction.FromId(Id);

    public override EntitySnapshot ToPuppet()
    {
        return new UnitPuppetSnapshot
        {
            Id = Id,
            IsRemoved = IsRemoved,
            Position = Position,
            Azimuth = Azimuth,
            IsDead = IsDead,
            IsMoving = IsMoving,
            Stats = Stats,
            AbilitySlotStates = AbilitySlotStates,
            AbilityUseProgress = AbilityUseProgress,
            StatusesContexts = StatusesContexts.ToDictionary(it => it.Key, it => it.Value.ToPuppet()).AsReadOnly(),
            InventorySnapshot = InventorySnapshot,
            ControllingPlayerIds = ControllingPlayerId == null ? [] : [ControllingPlayerId.Value],
            EquipmentInventory = EquipmentInventory
        };
    }

    public override EntityDelta DeltaFrom(UnitSnapshot? from) =>
        throw new NotSupportedException();
}
