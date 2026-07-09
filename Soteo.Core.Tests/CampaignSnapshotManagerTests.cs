using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute.ExceptionExtensions;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Core.Models;
using Soteo.Core.Services;
using Soteo.Core.Services.Repositories;

namespace Soteo.Core.Tests;

public sealed class CampaignSnapshotManagerTests
{
    private readonly UserRepository _userRepo;
    private readonly PlayerCharacterTrackerRepository _trackerRepo;
    private readonly FakePacketSender _packetSender;
    private readonly FakeTimeProvider _timeProvider;
    private readonly FakeConsistencyValidator _consistencyValidator;
    private readonly CampaignSnapshotManager _sut;
    
    public CampaignSnapshotManagerTests()
    {
        _userRepo = new UserRepository();
        _trackerRepo = new PlayerCharacterTrackerRepository();
        _packetSender = new FakePacketSender(() => _sut.Required);
        _timeProvider = new FakeTimeProvider();
        _consistencyValidator = new FakeConsistencyValidator();
        _sut =
            new CampaignSnapshotManager(_packetSender, _userRepo, _trackerRepo, _timeProvider, _consistencyValidator);
    }

    [Fact]
    public async Task CreatedSnapshotContainsUsersFromRepository()
    {
        User user1 = CreatePlayer();
        User user2 = CreatePlayer();
        
        CampaignSnapshot snapshot = await _sut.CreateSnapshotAsync();
        
        snapshot.CampaignServer.Users[user1.Id].Should().Be(user1.CreateSnapshot());
        snapshot.CampaignServer.Users[user2.Id].Should().Be(user2.CreateSnapshot());
    }
    
    [Fact]
    public async Task SnapshotContainsPlayerCharactersFromRepository()
    {
        var char1 = CreatePlayerCharacterInShard(Guid.NewGuid());
        var char2 = CreatePlayerCharacterInShard(null);
        
        CampaignSnapshot snapshot = await _sut.CreateSnapshotAsync();
        
        snapshot.CampaignServer.PlayerCharacterTrackers[char1.Id].Should().Be(char1.CreateSnapshot());
        snapshot.CampaignServer.PlayerCharacterTrackers[char2.Id].Should().Be(char2.CreateSnapshot());
    }
    
    [Fact]
    public async Task SnapshotContainsShardSnapshotsSentByShardServers()
    {
        var shard1 = CreateShard();
        var shard2 = CreateShard();
        
        CampaignSnapshot snapshot = await _sut.CreateSnapshotAsync();
        
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
        CreateShard();
        CreateShard();
        
        _packetSender.DuplicateResponse = true;
        
        await _sut.Awaiting(it => it.CreateSnapshotAsync()).Should().ThrowAsync<InvalidOperationException>(); 
    }
    
    [Fact]
    public async Task NotReceivingAllShardSnapshotsInTimeThrows()
    {
        CreateShard();
        CreateUnresponsiveShard();
        
        var assertTask = FluentActions.Awaiting(_sut.CreateSnapshotAsync).Should().ThrowAsync<TimeoutException>();
        _timeProvider.Advance(TimeSpan.FromSeconds(CampaignSnapshotManager.ShardServerResponseTimeout * 1.1));
        await assertTask;
    }
    
    [Fact]
    public async Task AlwaysFailingConsistencyValidationThrows()
    {
        CreateShard();
        _consistencyValidator.FailuresRemaining = int.MaxValue;
        
        var assertTask = FluentActions.Awaiting(_sut.CreateSnapshotAsync).Should().ThrowAsync<Exception>();
        for (int i = 0; i < 510; i++)
            _timeProvider.Advance(TimeSpan.FromDays(365));
        await assertTask;
    }
    
    [Fact]
    public async Task FailingConsistencyValidationRetriesAfterDelayUntilSuccess()
    {
        CreateShard();
        _consistencyValidator.FailuresRemaining = 2;

        Task actTask = _sut.CreateSnapshotAsync();
        
        _consistencyValidator.FailuresRemaining.Should().Be(1);
        _timeProvider.Advance(TimeSpan.FromSeconds(CampaignSnapshotManager.InconsistencyRetryDelay * 1.1));
        _consistencyValidator.FailuresRemaining.Should().Be(0);
        _timeProvider.Advance(TimeSpan.FromSeconds(CampaignSnapshotManager.InconsistencyRetryDelay * 1.1));
        _consistencyValidator.FailuresRemaining.Should().Be(-1);
        actTask.Status.Should().Be(TaskStatus.RanToCompletion);
    }
    
    [Fact]
    public async Task SnapshotReplicationPopulatesRepositories()
    {
        // Arrange
        
        var shardId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var characterId = Guid.NewGuid();
        
        var snapshot = new CampaignSnapshot
        {
            CampaignServer = new CampaignServerSnapshot
            {
                Users = new Dictionary<Guid, UserSnapshot>
                {
                    [shardId] = new()
                    {
                        Id = shardId,
                        IsConnected = false,
                        IsPlayer = false,
                        IsShard = true,
                    },
                    [playerId] = new()
                    {
                        Id = playerId,
                        IsConnected = false,
                        IsPlayer = true,
                        IsShard = false,
                    }
                }.ToImmutableDictionary(),
                PlayerCharacterTrackers = new Dictionary<Guid, PlayerCharacterTrackerSnapshot>
                {
                    [characterId] = new()
                    {
                        Id = characterId,
                        PlayerId = playerId,
                        ShardId = shardId,
                    },
                }.ToImmutableDictionary(),
            },
            Shards = ImmutableDictionary<Guid, ShardSnapshot>.Empty,
        };
        
        // Act
        
        await _sut.ReplicateSnapshotAsync(snapshot);
        
        // Assert
        
        _userRepo.Should().HaveCount(2);
        var expectedShard = new User
        {
            Id = shardId,
            IsConnected = false,
            IsPlayer = false,
            IsShard = true,
        };
        _userRepo.Should().ContainValue(expectedShard);
        var expectedPlayer = new User
        {
            Id = playerId,
            IsConnected = false,
            IsPlayer = true,
            IsShard = false,
        };
        _userRepo.Should().ContainValue(expectedPlayer);
        
        _trackerRepo.Should().HaveCount(1);
        var expectedCharTracker = new PlayerCharacterTracker
        {
            Id = characterId,
            Player = expectedPlayer,
            ShardId = shardId,
        };
        _trackerRepo.Should().ContainValue(expectedCharTracker);
    }
    
    [Fact]
    public async Task SnapshotReplicationSendsShardSnapshotsToShardServersAndWaitsForResponse()
    {
        (Guid shard1Id, Guid shard2Id, CampaignSnapshot snapshot) = CreateSnapshotWith2ShardSnapshots();
        
        Task task = _sut.ReplicateSnapshotAsync(snapshot);
        
        _packetSender.SendHistory.Should().BeEquivalentTo
        ([
            (
                new ShardSnapshotPacket { Snapshot = snapshot.Shards[shard1Id] },
                shard1Id
            ),
            (
                new ShardSnapshotPacket { Snapshot = snapshot.Shards[shard2Id] },
                shard2Id
            ),
        ]);
        
        task.IsCompleted.Should().BeFalse();
        _sut.ReceiveShardSnapshotReplicatedPacket(shard1Id);
        task.IsCompleted.Should().BeFalse();
        _sut.ReceiveShardSnapshotReplicatedPacket(shard2Id);
        task.IsCompleted.Should().BeTrue();
    }
    
    [Fact]
    public void ReceivingUnrequestedShardSnapshotReplicatedPacketThrows()
    {
        _sut.Invoking(it => it.ReceiveShardSnapshotReplicatedPacket(Guid.NewGuid()))
            .Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public void ReceivingDuplicateShardSnapshotReplicatedPacketThrows()
    {
        (Guid shard1Id, Guid shard2Id, CampaignSnapshot snapshot) = CreateSnapshotWith2ShardSnapshots();
        
        Task task = _sut.ReplicateSnapshotAsync(snapshot);
        
        _sut.ReceiveShardSnapshotReplicatedPacket(shard1Id);
        _sut.Invoking(it => it.ReceiveShardSnapshotReplicatedPacket(shard1Id))
            .Should().Throw<InvalidOperationException>();
    }
    
    [Fact]
    public async Task NeverGettingSnapshotReplicatedPacketThrows()
    {
        (Guid shard1Id, Guid shard2Id, CampaignSnapshot snapshot) = CreateSnapshotWith2ShardSnapshots();
        
        Task task = _sut.ReplicateSnapshotAsync(snapshot);
        
        for (int i = 0; i < 510; i++)
            _timeProvider.Advance(TimeSpan.FromDays(365));
        
        await FluentActions.Awaiting(() => task).Should().ThrowAsync<TimeoutException>();
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
    
    private PlayerCharacterTracker CreatePlayerCharacterInShard(Guid? shardId)
    {
        var character = new PlayerCharacterTracker { Id = Guid.NewGuid(), ShardId = shardId };
        _trackerRepo.Add(character);
        return character;
    }
    
    private (Guid Shard1Id, Guid Shard2Id, CampaignSnapshot Snapshot) CreateSnapshotWith2ShardSnapshots()
    {
        var shard1Id = Guid.NewGuid();
        var shard2Id = Guid.NewGuid();
        var snapshot = new CampaignSnapshot
        {
            CampaignServer = new CampaignServerSnapshot
            {
                Users = ImmutableDictionary<Guid, UserSnapshot>.Empty,
                PlayerCharacterTrackers = ImmutableDictionary<Guid, PlayerCharacterTrackerSnapshot>.Empty,
            },
            Shards = new Dictionary<Guid, ShardSnapshot>
            {
                [shard1Id] = new()
                {
                    Tick = 123,
                    Entities = ImmutableDictionary<Guid, EntitySnapshot>.Empty,
                },
                [shard2Id] = new()
                {
                    Tick = 321,
                    Entities = ImmutableDictionary<Guid, EntitySnapshot>.Empty,
                },
            },
        };
        return (shard1Id, shard2Id, snapshot);
    }
    
    private sealed class FakePacketSender(Func<CampaignSnapshotManager> sut) : IFromCampaignServerPacketSender
    {
        public IDictionary<Guid, ShardSnapshot> ShardSnapshots { get; } =
            new Dictionary<Guid, ShardSnapshot>();
        
        public bool DuplicateResponse { get; set; }
        
        public List<(Packet Packet, Guid ReceiverId)> SendHistory { get; } = [];
        
        public void SendTo(Packet packet, params IEnumerable<Guid> receiverIds)
        {
            foreach (Guid receiverId in receiverIds)
                SendHistory.Add((packet, receiverId));
            
            if (packet is ShardSnapshotRequestPacket)
            {
                foreach (Guid id in receiverIds)
                {
                    if (!ShardSnapshots.TryGetValue(id, out ShardSnapshot? snapshot)) continue;
                    for (int i = 0; i < (DuplicateResponse ? 2 : 1); i++)
                        sut().ReceiveShardSnapshotPacket(new ShardSnapshotPacket { Snapshot = snapshot }, id);
                }
            }
        }

        public void BroadcastToShardServers(Packet packet) => SendTo(packet, ShardSnapshots.Keys);
        public void BroadcastToAll(Packet packet) => throw new NotSupportedException();
        public void BroadcastToClients(Packet packet) => throw new NotSupportedException();
        public void RelayFrom(RelayedPacket packet, Guid senderId) => throw new NotSupportedException();
    }
    
    private sealed class FakeConsistencyValidator : ICampaignSnapshotCrossServerConsistencyValidator
    {
        public int FailuresRemaining { get; set; }
        
        public bool IsConsistent(CampaignSnapshot snapshot) => FailuresRemaining-- <= 0;
    }
}
