using System.Collections.Immutable;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services;

public sealed class PersistenceService
(
    IFromCampaignServerPacketSender packetSender,
    IUserRepository userRepo,
    IPlayerCharacterRepository charRepo,
    TimeProvider timeProvider,
    ICampaignSnapshotCrossServerConsistencyValidator consistencyValidator
) : IShardSnapshotPacketReceiver
{
    public const double ShardServerSnapshotRequestTimeout = 10;
    public const int InconsistencyRetryMaxCount = 10;
    public const int InconsistencyRetryDelay = 5;
    
    private readonly Dictionary<Guid, TaskCompletionSource<ShardSnapshot>> _shardSnapshotTcs = [];
    private bool _isSaving;
    
    public async Task<CampaignSnapshot> SaveAsync()
    {
        if (_isSaving)
            throw new InvalidOperationException("Save is already in progress");
        _isSaving = true;

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
            _isSaving = false;
            _shardSnapshotTcs.Clear();
        }
    }
    
    private async Task<CampaignSnapshot> CreateUnvalidatedCampaignSnapshot()
    {
        var campaignServerSnapshot = new CampaignServerSnapshot
        {
            PlayerCharacterTrackers = charRepo.CreateSnapshot(),
            Users = userRepo.CreateSnapshot(),
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
        Task timeout = timeProvider.Delay(TimeSpan.FromSeconds(ShardServerSnapshotRequestTimeout));
        Task completedTask =
            await Task.WhenAny(timeout, Task.WhenAll(_shardSnapshotTcs.Values.Select(it => it.Task)));
        if (completedTask == timeout) throw ShardSnapshotTimeoutException();
        return _shardSnapshotTcs.ToImmutableDictionary(it => it.Key, it => it.Value.Task.Result);
    }
    
    private Exception ShardSnapshotTimeoutException()
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

    public async Task LoadAsync(CampaignSnapshot snapshot)
    {
        throw new NotImplementedException();
    }
}
