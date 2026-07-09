using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Models;

namespace Soteo.Core.Interfaces;

public interface IPlayerCharacterRepository : IDictionary<Guid, PlayerCharacterTracker> // todo rename
{
    void Add(PlayerCharacterTracker playerCharacter);
    IReadOnlyDictionary<Guid, PlayerCharacterTrackerSnapshot> CreateSnapshot();

    void ReplicateSnapshot
    (
        IReadOnlyDictionary<Guid, PlayerCharacterTrackerSnapshot> snapshot,
        IUserRepository userRepo
    );
}
