namespace Soteo.Core.Synchronization.Deltas;

public sealed record UnitPuppetSnapshotDelta : EntitySnapshotDelta
{
    public required Delta<bool> IsDead { get; init; }
    public required Delta<bool> IsMoving { get; init; }
    public required DictionaryDelta<Stat, double> Stats { get; init; }
    public required DictionaryDelta<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; init; }
    public required Delta<AbilityUseProgress?> AbilityUseProgress { get; init; }
    public required DictionaryDelta<Guid, PuppetStatusContext> Statuses { get; init; }
    public required ListDelta<Guid> ControllingPlayerIds { get; init; }
    public required InventoryDelta Inventory { get; init; }
    public required EquipmentInventoryDelta EquipmentInventory { get; init; }

    public override bool HasChanged
    {
        get
        {
            return base.HasChanged || IsDead.HasChanged || IsMoving.HasChanged || Stats.HasChanged ||
                AbilitySlotStates.HasChanged || AbilityUseProgress.HasChanged || Statuses.HasChanged ||
                Inventory.HasChanged || ControllingPlayerIds.HasChanged || EquipmentInventory.HasChanged;
        }
    }
}
