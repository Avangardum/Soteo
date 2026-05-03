using Soteo.CampaignServer.GameState.DataObjects;

namespace Soteo.CampaignServer.Interfaces;

public interface ICharacterRepository : IDictionary<Guid, Character>
{
    void Add(Character character);
}