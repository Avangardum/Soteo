using Soteo.Core.CampaignServerState.DataObjects;
using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Interfaces;

public interface IPlayerCharacterRepository : IDictionary<Guid, PlayerCharacterTracker>
{
    void Add(PlayerCharacterTracker playerCharacter);
    IReadOnlyDictionary<Guid, PlayerCharacterSnapshot> CreateSnapshot();
}
