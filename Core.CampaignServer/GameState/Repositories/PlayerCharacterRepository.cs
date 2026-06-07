using System.Collections.Immutable;
using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;

namespace Soteo.Core.CampaignServer.GameState.Repositories;

public sealed class PlayerCharacterRepository : Dictionary<Guid, PlayerCharacter>, IPlayerCharacterRepository
{
    public void Add(PlayerCharacter playerCharacter) => Add(playerCharacter.Id, playerCharacter);
    
    public IReadOnlyDictionary<Guid, PlayerCharacter> CreateSnapshot() =>
        this.ToImmutableDictionary(it => it.Key, it => it.Value with { });
}
