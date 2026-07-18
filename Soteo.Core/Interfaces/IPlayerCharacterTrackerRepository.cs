using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Models;

namespace Soteo.Core.Interfaces;

public interface IPlayerCharacterTrackerRepository : IDictionary<Guid, PlayerCharacterTracker>
{
    void Add(PlayerCharacterTracker playerCharacter);
    IReadOnlyDictionary<Guid, PlayerCharacterTrackerSnapshot> ToSnapshot();

    void ReplicateSnapshot
    (
        IReadOnlyDictionary<Guid, PlayerCharacterTrackerSnapshot> snapshot,
        IUserRepository userRepo
    );
}
