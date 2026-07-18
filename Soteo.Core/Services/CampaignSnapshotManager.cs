using System.Collections.Immutable;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Util;

namespace Soteo.Core.Services;

public sealed class CampaignSnapshotManager
(
    IFromCampaignServerPacketSender packetSender,
    IUserRepository userRepo,
    IPlayerCharacterTrackerRepository trackerRepo,
    TimeProvider timeProvider,
    ICampaignSnapshotCrossServerConsistencyValidator consistencyValidator
) : ICampaignServerPersistencePacketReceiver
{
    public const double ShardServerResponseTimeout = 10;
    public const int InconsistencyRetryMaxCount = 10;
    public const int InconsistencyRetryDelay = 5;
    
    private readonly Dictionary<Guid, TaskCompletionSource<ShardSnapshot>> _shardSnapshotTcs = [];
    private readonly Dictionary<Guid, TaskCompletionSource> _shardSnapshotReplicatedTcs = [];
    private readonly SemaphoreSlim _mutex = new(1);
    
    public async Task<CampaignSnapshot> CreateSnapshotAsync()
    {
        await _mutex.WaitAsync();

        try
        {
            for (int i = 0; i < InconsistencyRetryMaxCount; i++)
            {
                CampaignSnapshot snapshot = await CreateUnvalidatedCampaignSnapshot();
                if (consistencyValidator.IsConsistent(snapshot)) return snapshot;
                await timeProvider.Delay(TimeSpan.FromSeconds(InconsistencyRetryDelay));
            }

            throw new Exception("Failed to create a consistent snapshot");
        }
        finally
        {
            _shardSnapshotTcs.Clear();
            _mutex.Release();
        }
    }
    
    private async Task<CampaignSnapshot> CreateUnvalidatedCampaignSnapshot()
    {
        var campaignServerSnapshot = new CampaignServerSnapshot
        {
            PlayerCharacterTrackers = trackerRepo.ToSnapshot(),
            Users = userRepo.ToSnapshot(),
        };

        return new CampaignSnapshot
        {
            CampaignServer = campaignServerSnapshot,
            Shards = await GetShardSnapshotsAsync(campaignServerSnapshot),
        };
    }
    
    private async Task<IReadOnlyDictionary<Guid, ShardSnapshot>> GetShardSnapshotsAsync
    (
        CampaignServerSnapshot campaignServerSnapshot
    )
    {
        _shardSnapshotTcs.Clear();
        foreach (UserSnapshot userSnapshot in campaignServerSnapshot.Users.Values)
            if (userSnapshot.IsShard)
                _shardSnapshotTcs[userSnapshot.Id] = new TaskCompletionSource<ShardSnapshot>();
        packetSender.BroadcastToShardServers(new ShardSnapshotRequestPacket());
        Task timeout = timeProvider.Delay(TimeSpan.FromSeconds(ShardServerResponseTimeout));
        Task completedTask =
            await Task.WhenAny(timeout, Task.WhenAll(_shardSnapshotTcs.Values.Select(it => it.Task)));
        if (completedTask == timeout) throw ShardServerSnapshotCreationTimeoutException();
        return _shardSnapshotTcs.ToImmutableDictionary(it => it.Key, it => it.Value.Task.Result);
    }
    
    private Exception ShardServerSnapshotCreationTimeoutException()
    {
        string timedOutShardIds = _shardSnapshotTcs
            .Where(it => !it.Value.Task.IsCompleted)
            .Select(it => it.Key)
            .JoinToString(", ");
        return new TimeoutException($"The following shard servers did not respond: {timedOutShardIds}");
    }

    public void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet, Guid senderId)
    {
        if (!_shardSnapshotTcs.TryGetValue(senderId, out TaskCompletionSource<ShardSnapshot>? tcs))
            throw new InvalidOperationException($"No packet was requested from {senderId}");
        if (!tcs.TrySetResult(packet.Snapshot))
            throw new InvalidOperationException($"Duplicate packet from {senderId}");
    }

    public async Task ReplicateSnapshotAsync(CampaignSnapshot snapshot)
    {
        await _mutex.WaitAsync();
        
        try
        {
            userRepo.ReplicateSnapshot(snapshot.CampaignServer.Users);
            trackerRepo.ReplicateSnapshot(snapshot.CampaignServer.PlayerCharacterTrackers, userRepo);

            foreach ((Guid shardId, ShardSnapshot shardSnapshot) in snapshot.Shards)
            {
                _shardSnapshotReplicatedTcs[shardId] = new TaskCompletionSource();
                var packet = new ShardSnapshotPacket { Snapshot = shardSnapshot };
                packetSender.SendTo(packet, shardId);
            }
            
            Task timeout = timeProvider.Delay(TimeSpan.FromSeconds(ShardServerResponseTimeout));
            Task completedTask =
                await Task.WhenAny(timeout, Task.WhenAll(_shardSnapshotReplicatedTcs.Values.Select(it => it.Task)));
            if (completedTask == timeout) throw ShardServerSnapshotReplicationTimeoutException();
        }
        finally
        {
            _shardSnapshotReplicatedTcs.Clear();
            _mutex.Release();
        }
    }
    
    private Exception ShardServerSnapshotReplicationTimeoutException()
    {
        string timedOutShardIds = _shardSnapshotReplicatedTcs
            .Where(it => !it.Value.Task.IsCompleted)
            .Select(it => it.Key)
            .JoinToString(", ");
        return new TimeoutException($"The following shard servers did not respond: {timedOutShardIds}");
    }

    public void ReceiveShardSnapshotReplicatedPacket(Guid senderId)
    {
        if (!_shardSnapshotReplicatedTcs.TryGetValue(senderId, out TaskCompletionSource tcs))
            throw new InvalidOperationException($"No packet was requested from {senderId}");
        if (!tcs.TrySetResult())
            throw new InvalidOperationException($"Duplicate packet from {senderId}");
    }
}
