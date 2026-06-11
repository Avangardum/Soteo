using System.Collections.Immutable;
using Soteo.Core.CampaignServerState.DataObjects;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;

namespace Soteo.Core.CampaignServer.Tests;

public sealed class CampaignSnapshotCrossServerConsistencyValidatorTests
{
    private static readonly Guid Player1Id = Guid.NewGuid(); 
    private static readonly Guid Player2Id = Guid.NewGuid(); 
    private static readonly Guid Player3Id = Guid.NewGuid();
    private static readonly Guid Char1Id = Guid.NewGuid(); 
    private static readonly Guid Char2Id = Guid.NewGuid(); 
    private static readonly Guid Char3Id = Guid.NewGuid();
    private static readonly Guid Shard1Id = Guid.NewGuid(); 
    private static readonly Guid Shard2Id = Guid.NewGuid(); 
    private static readonly Guid Shard3Id = Guid.NewGuid(); 
    
    private static readonly CampaignSnapshot ConsistentSnapshot = new()
    {
        CampaignServer = new CampaignServerSnapshot
        {
            Users = new Dictionary<Guid, UserSnapshot>
            {
                [Player1Id] = CreatePlayerUserSnapshot(Player1Id),
                [Player2Id] = CreatePlayerUserSnapshot(Player2Id),
                [Player3Id] = CreatePlayerUserSnapshot(Player3Id),
                [Shard1Id] = CreateShardUserSnapshot(Shard1Id),
                [Shard2Id] = CreateShardUserSnapshot(Shard2Id),
                [Shard3Id] = CreateShardUserSnapshot(Shard3Id),
            }.ToImmutableDictionary(),
            Characters = new Dictionary<Guid, PlayerCharacterSnapshot>
            {
                [Char1Id] = new()
                {
                    Id = Char1Id,
                    PlayerId = Player1Id,
                    ShardId = Shard1Id,
                },
                [Char2Id] = new()
                {
                    Id = Char2Id,
                    PlayerId = Player2Id,
                    ShardId = Shard2Id,
                },
                [Char3Id] = new()
                {
                    Id = Char3Id,
                    PlayerId = Player3Id,
                    ShardId = null,
                },
            }.ToImmutableDictionary(),
        },
        Shards = new Dictionary<Guid, ShardSnapshot>
        {
            [Shard1Id] = new()
            {
                Tick = 0,
                Entities = new Dictionary<Guid, EntitySnapshot>
                {
                    [Char1Id] = CreatePlayerCharacterEntitySnapshot(Char1Id)
                }.ToImmutableDictionary(),
            },
            [Shard2Id] = new()
            {
                Tick = 0,
                Entities = new Dictionary<Guid, EntitySnapshot>
                {
                    [Char2Id] = CreatePlayerCharacterEntitySnapshot(Char2Id)
                }.ToImmutableDictionary(),
            },
            [Shard3Id] = new()
            {
                Tick = 0,
                Entities = ImmutableDictionary<Guid, EntitySnapshot>.Empty,
            },
        }.ToImmutableDictionary(),
    };
    
    // [Fact]
    // public void ConsistentSnapshotPassesValidation()
    // {
    //     
    // }
    
    private static UserSnapshot CreatePlayerUserSnapshot(Guid id)
    {
        return new()
        {
            Id = id,
            IsConnected = true,
            IsPlayer = true,
            IsShard = false,
        };
    }
    
    private static UserSnapshot CreateShardUserSnapshot(Guid id)
    {
        return new()
        {
            Id = id,
            IsConnected = true,
            IsPlayer = false,
            IsShard = true,
        };
    }
    
    private static UnitSnapshot CreatePlayerCharacterEntitySnapshot(Guid id)
    {
        return new()
        {
            Id = id,
            IsDead = false,
            IsMoving = false,
            Stats = ImmutableDictionary<Stat, double>.Empty,
            AbilitySlotStates = ImmutableDictionary<AbilitySlot, AbilitySlotState>.Empty,
            AbilityUseProgress = null,
            Statuses = ImmutableDictionary<Guid, DeflatedStatusContext>.Empty,
            IsRemoved = false,
            Position = default,
            Azimuth = 0,
        };
    }
}
