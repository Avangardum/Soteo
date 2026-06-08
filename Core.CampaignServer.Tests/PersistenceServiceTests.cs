using AwesomeAssertions;
using NSubstitute;
using Soteo.Core.CampaignServer.Dto;
using Soteo.Core.CampaignServer.Dto.Snapshots;
using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.GameState.Repositories;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.CampaignServer.Services;
using Soteo.Core.Shared.Dto.Snapshots;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.Tests;

public sealed class PersistenceServiceTests
{
    private readonly UserRepository _userRepo;
    private readonly PlayerCharacterRepository _charRepo;
    private readonly IPacketSender _packetSender;
    private readonly PersistenceService _sut;
    
    public PersistenceServiceTests()
    {
        _userRepo = new UserRepository();
        _charRepo = new PlayerCharacterRepository();
        _packetSender = new FakePacketSender();
        _sut = new PersistenceService(_packetSender, _userRepo, _charRepo);
    }

    [Fact]
    public async Task SavedSnapshotContainsUsersFromRepository()
    {
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            IsConnected = false,
            IsPlayer = true,
            IsShard = false
        };
        var user2 = new User
        {
            Id = Guid.NewGuid(),
            IsConnected = true,
            IsPlayer = false,
            IsShard = true
        };
        _userRepo[user1.Id] = user1;
        _userRepo[user2.Id] = user2;
        
        CampaignSnapshot snapshot = await _sut.SaveAsync();
        
        snapshot.CampaignServer.Users[user1.Id].Should().Be(user1.CreateSnapshot());
        snapshot.CampaignServer.Users[user2.Id].Should().Be(user2.CreateSnapshot());
    }
    
    [Fact]
    public async Task SavedSnapshotContainsPlayerCharactersFromRepository()
    {
        var char1 = new PlayerCharacter { Id = Guid.NewGuid(), ShardId = Guid.NewGuid() };
        var char2 = new PlayerCharacter { Id = Guid.NewGuid(), ShardId = null };
        _charRepo.Add(char1);
        _charRepo.Add(char2);
        
        CampaignSnapshot snapshot = await _sut.SaveAsync();
        
        snapshot.CampaignServer.Characters[char1.Id].Should().Be(char1.CreateSnapshot());
        snapshot.CampaignServer.Characters[char2.Id].Should().Be(char2.CreateSnapshot());
    }
    
    [Fact]
    public async Task SavedSnapshotContainsShardSnapshotsSentByShardServers()
    {
        var shard1 = new User{ Id = Guid.NewGuid(), IsConnected = true, IsPlayer = false, IsShard = true };
        var shard2 = new User{ Id = Guid.NewGuid(), IsConnected = true, IsPlayer = false, IsShard = true };
        
        CampaignSnapshot snapshot = await _sut.SaveAsync();
        
        snapshot.Shards.Should().NotBeEmpty();
    }
    
    private sealed class FakePacketSender() : IPacketSender
    {
        public IDictionary<Guid, ShardSnapshot> ShardSnapshots { get; } =
            new Dictionary<Guid, ShardSnapshot>();
        
        public void SendTo(Packet packet, Guid receiverId)
        {
            throw new NotImplementedException();
        }

        public void Broadcast(Packet packet)
        {
            foreach (Guid id in ShardSnapshots.Keys)
                SendTo(packet, id);
        }

        public void RelayFrom(RelayedPacket packet, Guid senderId) =>
            throw new NotSupportedException();
    }
}
