using Soteo.Core.CampaignServerState.DataObjects;
using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Interfaces;

public interface IPlayerCharacterRepository : IDictionary<Guid, PlayerCharacter>
{
    void Add(PlayerCharacter playerCharacter);
    IReadOnlyDictionary<Guid, PlayerCharacterSnapshot> CreateSnapshot();
}
