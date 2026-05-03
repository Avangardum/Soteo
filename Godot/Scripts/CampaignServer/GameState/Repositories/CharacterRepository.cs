using Soteo.CampaignServer.GameState.DataObjects;
using Soteo.CampaignServer.Interfaces;

namespace Soteo.CampaignServer.GameState.Repositories;

public sealed class CharacterRepository : Dictionary<Guid, Character>, ICharacterRepository
{
    public void Add(Character character) => Add(character.Id, character);
}