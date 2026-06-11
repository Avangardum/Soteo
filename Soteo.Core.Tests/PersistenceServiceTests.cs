using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Soteo.Core.CampaignServerState.DataObjects;
using Soteo.Core.CampaignServerState.Repositories;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;
using Soteo.Core.Services;

namespace Soteo.Core.Tests;

public sealed class PersistenceServiceTests
{
    private readonly UserRepository _userRepo;
    private readonly PlayerCharacterRepository _charRepo;
    private readonly FakePacketSender _packetSender;
    private readonly FakeTimeProvider _timeProvider;
    private readonly FakeConsistencyValidator _consistencyValidator;
    private readonly PersistenceService _sut;
    
    public PersistenceServiceTests()
    {
        _userRepo = new UserRepository();
        _charRepo = new PlayerCharacterRepository();
        _packetSender =
            new FakePacketSender((packet, senderId) => _sut.Required.ReceiveShardSnapshotPacket(packet, senderId));
        _timeProvider = new FakeTimeProvider();
        _consistencyValidator = new FakeConsistencyValidator();
        _sut = new PersistenceService(_packetSender, _userRepo, _charRepo, _timeProvider, _consistencyValidator);
    }

    [Fact]
    public async Task SavedSnapshotContainsUsersFromRepository()
    {
        User user1 = CreatePlayer();
        User user2 = CreatePlayer();
        
        CampaignSnapshot snapshot = await _sut.SaveAsync();
        
        snapshot.CampaignServer.Users[user1.Id].Should().Be(user1.CreateSnapshot());
        snapshot.CampaignServer.Users[user2.Id].Should().Be(user2.CreateSnapshot());
    }
    
    [Fact]
    public async Task SavedSnapshotContainsPlayerCharactersFromRepository()
    {
        var char1 = CreatePlayerCharacterInShard(Guid.NewGuid());
        var char2 = CreatePlayerCharacterInShard(null);
        
        CampaignSnapshot snapshot = await _sut.SaveAsync();
        
        snapshot.CampaignServer.PlayerCharacters[char1.Id].Should().Be(char1.CreateSnapshot());
        snapshot.CampaignServer.PlayerCharacters[char2.Id].Should().Be(char2.CreateSnapshot());
    }
    
    [Fact]
    public async Task SavedSnapshotContainsShardSnapshotsSentByShardServers()
    {
        var shard1 = CreateShard();
        var shard2 = CreateShard();
        
        CampaignSnapshot snapshot = await _sut.SaveAsync();
        
        snapshot.Shards[shard1.Id].Tick.Should().Be(shard1.Id.ToString()[^1]);
        snapshot.Shards[shard2.Id].Tick.Should().Be(shard2.Id.ToString()[^1]);
    }
    
    [Fact]
    public async Task ReceivingUnrequestedShardSnapshotPacketThrows()
    {
        _sut.Invoking
        (
            it => it.ReceiveShardSnapshotPacket
            (
                new ShardSnapshotPacket
                {
                    Snapshot = new ShardSnapshot
                    {
                        Tick = 123,
                        Entities = ImmutableDictionary<Guid, EntitySnapshot>.Empty,
                    }
                },
                Guid.NewGuid()
            )
        ).Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public async Task ReceivingDuplicatedShardSnapshotPacketThrows()
    {
        var shard = CreateShard();
        
        _packetSender.DuplicateResponse = true;
        
        await _sut.Awaiting(it => it.SaveAsync()).Should().ThrowAsync<InvalidOperationException>(); 
    }
    
    [Fact]
    public async Task NotReceivingAllShardSnapshotsInTimeThrows()
    {
        CreateShard();
        CreateUnresponsiveShard();
        
        var assertTask = FluentActions.Awaiting(_sut.SaveAsync).Should().ThrowAsync<TimeoutException>();
        _timeProvider.Advance(TimeSpan.FromSeconds(PersistenceService.ShardServerSnapshotRequestTimeout * 1.1));
        await assertTask;
    }
    
    [Fact]
    public async Task AlwaysFailingConsistencyValidationThrows()
    {
        CreateShard();
        _consistencyValidator.FailuresRemaining = int.MaxValue;
        
        var assertTask = FluentActions.Awaiting(_sut.SaveAsync).Should().ThrowAsync<Exception>();
        for (int i = 0; i < 510; i++)
            _timeProvider.Advance(TimeSpan.FromDays(365));
        await assertTask;
    }
    
    [Fact]
    public async Task FailingConsistencyValidationRetriesAfterDelayUntilSuccess()
    {
        CreateShard();
        _consistencyValidator.FailuresRemaining = 2;

        Task actTask = _sut.SaveAsync();
        
        _consistencyValidator.FailuresRemaining.Should().Be(1);
        _timeProvider.Advance(TimeSpan.FromSeconds(PersistenceService.InconsistencyRetryDelay * 1.1));
        _consistencyValidator.FailuresRemaining.Should().Be(0);
        _timeProvider.Advance(TimeSpan.FromSeconds(PersistenceService.InconsistencyRetryDelay * 1.1));
        _consistencyValidator.FailuresRemaining.Should().Be(-1);
        actTask.Status.Should().Be(TaskStatus.RanToCompletion);
    }
    
    [Fact]
    public async Task CocurrentSaveThrows()
    {
        CreateUnresponsiveShard();
        _ = _sut.SaveAsync();
        await FluentActions.Awaiting(_sut.SaveAsync).Should().ThrowAsync<InvalidOperationException>();
    }
    
    [Fact]
    public async Task SequentialSaveDoesNotThrow()
    {
        CreateShard();
        await _sut.SaveAsync();
        await _sut.SaveAsync();
    }
    
    private User CreatePlayer()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            IsConnected = false,
            IsPlayer = true,
            IsShard = false
        };
        _userRepo[user.Id] = user;
        return user;
    }
    
    private User CreateShard()
    {
        var shard = CreateUnresponsiveShard();
        var shard1Snapshot = new ShardSnapshot
        {
            Tick = shard.Id.ToString()[^1],
            Entities = ImmutableDictionary<Guid, EntitySnapshot>.Empty,
        };
        _packetSender.ShardSnapshots[shard.Id] = shard1Snapshot;
        return shard;
    }
    
    private User CreateUnresponsiveShard()
    {
        var shard = new User { Id = Guid.NewGuid(), IsConnected = true, IsPlayer = false, IsShard = true };
        _userRepo[shard.Id] = shard;
        return shard;
    }
    
    private PlayerCharacter CreatePlayerCharacterInShard(Guid? shardId)
    {
        var character = new PlayerCharacter { Id = Guid.NewGuid(), ShardId = shardId };
        _charRepo.Add(character);
        return character;
    }
    
    private sealed class FakePacketSender(Action<ShardSnapshotPacket, Guid> callback) : IFromCampaignServerPacketSender
    {
        public IDictionary<Guid, ShardSnapshot> ShardSnapshots { get; } =
            new Dictionary<Guid, ShardSnapshot>();
        
        public bool DuplicateResponse { get; set; }
        
        public void SendTo(Packet packet, params IEnumerable<Guid> receiverIds)
        {
            if (packet is not ShardSnapshotRequestPacket) return;
            foreach (Guid id in receiverIds)
            {
                if (!ShardSnapshots.TryGetValue(id, out ShardSnapshot? snapshot)) return;
                for (int i = 0; i < (DuplicateResponse ? 2 : 1); i++)
                    callback(new ShardSnapshotPacket { Snapshot = snapshot }, id);
            }
        }

        public void BroadcastToShardServers(Packet packet) => SendTo(packet, ShardSnapshots.Keys);

        public void BroadcastToShardServersAndClients(Packet packet) => throw new NotSupportedException();
        public void BroadcastToClients(Packet packet) => throw new NotSupportedException();
        public void RelayFrom(RelayedPacket packet, Guid senderId) => throw new NotSupportedException();
    }
    
    private sealed class FakeConsistencyValidator : ICampaignSnapshotCrossServerConsistencyValidator
    {
        public int FailuresRemaining { get; set; }
        
        public bool IsConsistent(CampaignSnapshot snapshot) => FailuresRemaining-- <= 0;
    }
}
