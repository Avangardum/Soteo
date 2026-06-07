using Soteo.Core.CampaignServer.GameState.DataObjects;

namespace Soteo.Core.CampaignServer.Interfaces;

public interface IPlayerCharacterRepository : IDictionary<Guid, PlayerCharacter>
{
    void Add(PlayerCharacter playerCharacter);
    IReadOnlyDictionary<Guid, PlayerCharacter> CreateSnapshot();
}
