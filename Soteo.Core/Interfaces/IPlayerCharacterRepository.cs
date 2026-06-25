using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Models;

namespace Soteo.Core.Interfaces;

public interface IPlayerCharacterRepository : IDictionary<Guid, PlayerCharacterTracker>
{
    void Add(PlayerCharacterTracker playerCharacter);
    IReadOnlyDictionary<Guid, PlayerCharacterSnapshot> CreateSnapshot();
}
