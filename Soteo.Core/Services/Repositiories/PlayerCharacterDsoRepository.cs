using System.Collections.Immutable;
using Soteo.Core.CampaignServerState.DataObjects;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;

namespace Soteo.Core.CampaignServerState.Repositories;

public sealed class PlayerCharacterDsoRepository : Dictionary<Guid, PlayerCharacterTracker>, IPlayerCharacterRepository
{
    public void Add(PlayerCharacterTracker playerCharacter) => Add(playerCharacter.Id, playerCharacter);
    
    public IReadOnlyDictionary<Guid, PlayerCharacterSnapshot> CreateSnapshot() =>
        this.ToImmutableDictionary(it => it.Key, it => it.Value.CreateSnapshot());
}
