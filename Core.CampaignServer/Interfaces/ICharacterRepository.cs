using Soteo.Core.CampaignServer.GameState.DataObjects;

namespace Soteo.Core.CampaignServer.Interfaces;

public interface ICharacterRepository : IDictionary<Guid, Character>
{
    void Add(Character character);
    IReadOnlyDictionary<Guid, Character> CreateSnapshot();
}
