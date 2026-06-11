using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services;

public sealed class CampaignSnapshotCrossServerConsistencyValidator : ICampaignSnapshotCrossServerConsistencyValidator
{
    public bool IsConsistent(CampaignSnapshot snapshot)
    {
        return DoPlayerCharactersWithShardIdExistInCorrespondingShardSnapshots(snapshot) &&
            DoPlayerCharactersInShardSnapshotsHaveCorrespondingShardId(snapshot);
    }

    private bool DoPlayerCharactersWithShardIdExistInCorrespondingShardSnapshots(CampaignSnapshot snapshot)
    {
        IEnumerable<PlayerCharacterSnapshot> deployedPlayerCharacters =
            snapshot.CampaignServer.PlayerCharacters.Values.Where(it => it.ShardId != null);
        foreach (PlayerCharacterSnapshot character in deployedPlayerCharacters)
        {
            ShardSnapshot shard = snapshot.Shards[character.ShardId.Required];
            if (!shard.Entities.ContainsKey(character.Id))
                return false;
        }
        
        return true;
    }
    
    private bool DoPlayerCharactersInShardSnapshotsHaveCorrespondingShardId(CampaignSnapshot snapshot)
    {
        foreach ((Guid shardId, ShardSnapshot shard) in snapshot.Shards)
        {
            foreach (UnitSnapshot unit in shard.Entities.Values.OfType<UnitSnapshot>())
            {
                if (!snapshot.CampaignServer.PlayerCharacters.TryGetValue(unit.Id, out PlayerCharacterSnapshot? character))
                    return false;
                if (character.ShardId != shardId)
                    return false;
            }
        }
        
        return true;
    }
}
