using AwesomeAssertions;
using NSubstitute;
using Soteo.Core.CampaignServer.Dto;
using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.GameState.Repositories;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.CampaignServer.Services;

namespace Soteo.Core.CampaignServer.Tests;

public sealed class PersistenceServiceTests
{
    private readonly UserRepository _userRepo;
    private readonly PlayerCharacterRepository _charRepo;
    private readonly PersistenceService _sut;
    
    public PersistenceServiceTests()
    {
        _userRepo = new UserRepository();
        _charRepo = new PlayerCharacterRepository();
        _sut = new PersistenceService(Substitute.For<IPacketSender>(), _userRepo, _charRepo);
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
        
        snapshot.CampaignServer.Users[user1.Id].Should().Be(user1);
        snapshot.CampaignServer.Users[user2.Id].Should().Be(user2);
    }
    
    [Fact]
    public async Task SavedSnapshotContainsPlayerCharactersFromRepository()
    {
        var char1 = new PlayerCharacter { Id = Guid.NewGuid(), ShardId = Guid.NewGuid() };
        var char2 = new PlayerCharacter { Id = Guid.NewGuid(), ShardId = null };
        _charRepo.Add(char1);
        _charRepo.Add(char2);
        
        CampaignSnapshot snapshot = await _sut.SaveAsync();
        
        snapshot.CampaignServer.Characters[char1.Id].Should().Be(char1);
        snapshot.CampaignServer.Characters[char2.Id].Should().Be(char2);
    }
    
    [Fact]
    public async Task SavedSnapshotDoesNotChangeWhenRepositoryStateChanges()
    {
        // Arrange
        
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
        
        var char1 = new PlayerCharacter { Id = Guid.NewGuid(), ShardId = Guid.NewGuid() };
        var char2 = new PlayerCharacter { Id = Guid.NewGuid(), ShardId = null };
        _charRepo.Add(char1);
        _charRepo.Add(char2);
        
        // Act
        
        CampaignSnapshot snapshot = await _sut.SaveAsync();
        
        _userRepo.Remove(user1.Id);
        _userRepo[user2.Id].IsConnected = false;
        
        _charRepo.Remove(char1.Id);
        _charRepo[char2.Id].ShardId = Guid.NewGuid();
        
        // Assert
        
        snapshot.CampaignServer.Users.Should().ContainKey(user1.Id);
        snapshot.CampaignServer.Users[user2.Id].IsConnected.Should().BeTrue();
        
        snapshot.CampaignServer.Characters.Should().ContainKey(char1.Id);
        snapshot.CampaignServer.Characters[char2.Id].ShardId.Should().BeNull();
    }
}
