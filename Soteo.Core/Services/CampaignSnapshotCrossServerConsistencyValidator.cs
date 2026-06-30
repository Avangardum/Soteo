using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services;

public sealed class CampaignSnapshotCrossServerConsistencyValidator : ICampaignSnapshotCrossServerConsistencyValidator
{
    public bool IsConsistent(CampaignSnapshot snapshot)
    {
        return
            DoShardUsersMatchShardSnapshots(snapshot) &&
            DoPlayerCharacterTrackersWithShardIdHaveCorrespondingCharsInShardSnapshots(snapshot) &&
            DoPlayerCharactersInShardSnapshotsHaveCorrespondingTrackers(snapshot);
    }

    private bool DoShardUsersMatchShardSnapshots(CampaignSnapshot snapshot)
    {
        IEnumerable<Guid> shardUserIds =
            snapshot.CampaignServer.Users.Where(it => it.Value.IsShard).Select(it => it.Key);
        IEnumerable<Guid> shardSnapshotIds = snapshot.Shards.Keys;
        return shardUserIds.SequenceEqual(shardSnapshotIds);
    }
    
    private bool DoPlayerCharacterTrackersWithShardIdHaveCorrespondingCharsInShardSnapshots(CampaignSnapshot snapshot)
    {
        IEnumerable<PlayerCharacterTrackerSnapshot> deployedPlayerCharacters =
            snapshot.CampaignServer.PlayerCharacterTrackers.Values.Where(it => it.ShardId != null);
        foreach (PlayerCharacterTrackerSnapshot character in deployedPlayerCharacters)
        {
            ShardSnapshot shard = snapshot.Shards[character.ShardId.Required];
            if (!shard.Entities.ContainsKey(character.Id)) return false;
        }
        
        return true;
    }
    
    private bool DoPlayerCharactersInShardSnapshotsHaveCorrespondingTrackers(CampaignSnapshot snapshot)
    {
        foreach ((Guid shardId, ShardSnapshot shard) in snapshot.Shards)
        {
            foreach (UnitSnapshot unit in shard.Entities.Values.OfType<UnitSnapshot>())
            {
                if (!snapshot.CampaignServer.PlayerCharacterTrackers.TryGetValue(unit.Id, out var character))
                    return false;
                if (character.ShardId != shardId)
                    return false;
            }
        }
        
        return true;
    }
}
