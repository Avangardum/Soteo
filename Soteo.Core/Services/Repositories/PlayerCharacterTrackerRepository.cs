using System.Collections.Immutable;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Core.Models;

namespace Soteo.Core.Services.Repositories;

/// <inheritdoc cref="IPlayerCharacterTrackerRepository" />
public sealed class PlayerCharacterTrackerRepository :
    Dictionary<Guid, PlayerCharacterTracker>, IPlayerCharacterTrackerRepository
{
    public void Add(PlayerCharacterTracker playerCharacter) => Add(playerCharacter.Id, playerCharacter);
    
    public IReadOnlyDictionary<Guid, PlayerCharacterTrackerSnapshot> ToSnapshot() =>
        this.ToImmutableDictionary(it => it.Key, it => it.Value.ToSnapshot());
    
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
