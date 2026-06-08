using System.Collections.Immutable;
using Soteo.Core.CampaignServer.Dto;
using Soteo.Core.CampaignServer.Dto.Snapshots;
using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared.Dto.Snapshots;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.Services;

public sealed class PersistenceService
(
    IPacketSender packetSender,
    IUserRepository userRepo,
    IPlayerCharacterRepository charRepo,
    TimeProvider timeProvider,
    PersistenceService.Options options
)
{
    private readonly Dictionary<Guid, TaskCompletionSource<ShardSnapshot>> _shardSnapshotTcs = [];
    
    public async Task<CampaignSnapshot> SaveAsync()
    {
        var campaignServerSnapshot = new CampaignServerSnapshot
        {
            Characters = charRepo.CreateSnapshot(),
            Users = userRepo.CreateSnapshot(),
        };
        
        _shardSnapshotTcs.Clear();
        foreach (UserSnapshot userSnapshot in campaignServerSnapshot.Users.Values)
            if (userSnapshot.IsShard)
                _shardSnapshotTcs[userSnapshot.Id] = new TaskCompletionSource<ShardSnapshot>();
        packetSender.BroadcastToShardServers(new ShardSnapshotRequestPacket());
        Task timeout = timeProvider.Delay(TimeSpan.FromSeconds(options.ShardServerSnapshotRequestTimeout));
        Task completedTask = await Task.WhenAny(timeout, Task.WhenAll(_shardSnapshotTcs.Values.Select(it => it.Task)));
        if (completedTask == timeout)
        {
            string timedOutShardIds = _shardSnapshotTcs
                .Where(it => !it.Value.Task.IsCompleted)
                .Select(it => it.Key)
                .JoinToString(", ");
            throw new TimeoutException($"The following shard servers did not respond: {timedOutShardIds}");
        }

        var result = new CampaignSnapshot
        {
            CampaignServer = campaignServerSnapshot,
            Shards = _shardSnapshotTcs.ToImmutableDictionary(it => it.Key, it => it.Value.Task.Result),
        };
        _shardSnapshotTcs.Clear();
        return result;
    }
    
    public async Task LoadAsync(CampaignSnapshot snapshot)
    {
        throw new NotImplementedException();
    }
    
    public void ReceiveShardSnapshotPacket(ShardSnapshotPacket packet, Guid senderId)
    {
        if (!_shardSnapshotTcs.TryGetValue(senderId, out TaskCompletionSource<ShardSnapshot>? tcs))
            throw new InvalidOperationException();
        tcs.SetResult(packet.Snapshot);
    }
    
    public sealed record Options
    {
        public double ShardServerSnapshotRequestTimeout { get; init; } = 10;
    }
}
