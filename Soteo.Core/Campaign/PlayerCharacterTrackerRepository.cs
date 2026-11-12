namespace Soteo.Core.Campaign;

/// <inheritdoc cref="IPlayerCharacterTrackerRepository" />
public sealed class PlayerCharacterTrackerRepository :
    Dictionary<Guid, PlayerCharacterTracker>, IPlayerCharacterTrackerRepository
{
    public void Add(PlayerCharacterTracker playerCharacter) => Add(playerCharacter.Id, playerCharacter);

    public IReadOnlyDictionary<Guid, PlayerCharacterTrackerSnapshot> ToSnapshot() =>
        this.ToDictionary(it => it.Key, it => it.Value.ToSnapshot()).AsReadOnly();

    public void ReplicateSnapshot
    (
        IReadOnlyDictionary<Guid, PlayerCharacterTrackerSnapshot> snapshot,
        IUserRepository userRepo
    )
    {
        Clear();
        foreach (PlayerCharacterTrackerSnapshot trackerSnapshot in snapshot.Values)
            Add(PlayerCharacterTracker.FromSnapshot(trackerSnapshot, userRepo));
    }
}
