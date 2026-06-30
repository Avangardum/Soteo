using System.Collections.Immutable;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Core.Models;

namespace Soteo.Core.Services.Repositiories;

public sealed class PlayerCharacterTrackerRepository : Dictionary<Guid, PlayerCharacterTracker>, IPlayerCharacterRepository
{
    public void Add(PlayerCharacterTracker playerCharacter) => Add(playerCharacter.Id, playerCharacter);
    
    public IReadOnlyDictionary<Guid, PlayerCharacterTrackerSnapshot> CreateSnapshot() =>
        this.ToImmutableDictionary(it => it.Key, it => it.Value.CreateSnapshot());
}
