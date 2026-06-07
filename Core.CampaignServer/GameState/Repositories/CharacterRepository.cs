using System.Collections.Immutable;
using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;

namespace Soteo.Core.CampaignServer.GameState.Repositories;

public sealed class CharacterRepository : Dictionary<Guid, Character>, ICharacterRepository
{
    public void Add(Character character) => Add(character.Id, character);
    
    public IReadOnlyDictionary<Guid, Character> CreateSnapshot() =>
        this.ToImmutableDictionary(it => it.Key, it => it.Value with { });
}
