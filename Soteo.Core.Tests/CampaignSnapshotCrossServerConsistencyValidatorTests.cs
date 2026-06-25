using System.Collections.Immutable;
using System.Numerics;
using AwesomeAssertions;
using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Services;

namespace Soteo.Core.Tests;

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
    private static readonly Guid ProjectileId = Guid.NewGuid(); 
    
    // todo add non-player units after they are introduced
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
            PlayerCharacters = new Dictionary<Guid, PlayerCharacterSnapshot>
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
                    [Char1Id] = CreatePlayerCharacterEntitySnapshot(Char1Id),
                    [ProjectileId] = CreateProjectileEntitySnapshot(ProjectileId), 
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
    
    [Fact]
    public void ConsistentSnapshotPassesValidation()
    {
        var sut = new CampaignSnapshotCrossServerConsistencyValidator();
        sut.IsConsistent(ConsistentSnapshot).Should().BeTrue();
    }
    
    [Fact]
    public void SnapshotWhereShardSnapshotMissesPlayerCharacterFailsValidation()
    {
        var sut = new CampaignSnapshotCrossServerConsistencyValidator();
        var snapshot = ConsistentSnapshot with
        {
            Shards = ConsistentSnapshot.Shards.With
            (
                new Dictionary<Guid, ShardSnapshot>
                {
                    [Shard1Id] = ConsistentSnapshot.Shards[Shard1Id] with
                    {
                        Entities = ImmutableDictionary<Guid, EntitySnapshot>.Empty,
                    },
                }
            ),
        };
        
        sut.IsConsistent(snapshot).Should().BeFalse();
    }
    
    [Fact]
    public void SnapshotWherePlayerCharacterIsInWrongShardSnapshotFailsValidation()
    {
        var sut = new CampaignSnapshotCrossServerConsistencyValidator();
        var snapshot = ConsistentSnapshot with
        {
            Shards = ConsistentSnapshot.Shards.With
            (
                new Dictionary<Guid, ShardSnapshot>
                {
                    [Shard1Id] = ConsistentSnapshot.Shards[Shard1Id] with
                    {
                        Entities = ConsistentSnapshot.Shards[Shard2Id].Entities,
                    },
                    [Shard2Id] = ConsistentSnapshot.Shards[Shard2Id] with
                    {
                        Entities = ConsistentSnapshot.Shards[Shard1Id].Entities,
                    },
                }
            ),
        };
        
        sut.IsConsistent(snapshot).Should().BeFalse();
    }
    
    [Fact]
    public void SnapshotWherePlayerCharacterIsInManyShardSnapshotsFailsValidation()
    {
        var sut = new CampaignSnapshotCrossServerConsistencyValidator();
        var snapshot = ConsistentSnapshot with
        {
            Shards = ConsistentSnapshot.Shards.With
            (
                new Dictionary<Guid, ShardSnapshot>
                {
                    [Shard1Id] = ConsistentSnapshot.Shards[Shard1Id] with
                    {
                        Entities = ConsistentSnapshot.Shards[Shard1Id].Entities
                            .Concat(ConsistentSnapshot.Shards[Shard2Id].Entities)
                            .ToImmutableDictionary(),
                    },
                }
            ),
        };
        
        sut.IsConsistent(snapshot).Should().BeFalse();
    }
    
    [Fact]
    public void SnapshotWhereUndeployedCharacterIsInShardSnapshotFailsValidation()
    {
        var sut = new CampaignSnapshotCrossServerConsistencyValidator();
        CampaignSnapshot snapshot = ConsistentSnapshot with
        {
            CampaignServer = ConsistentSnapshot.CampaignServer with
            {
                PlayerCharacters = ConsistentSnapshot.CampaignServer.PlayerCharacters.With
                (
                    new Dictionary<Guid, PlayerCharacterSnapshot>
                    {
                        [Char1Id] = ConsistentSnapshot.CampaignServer.PlayerCharacters[Char1Id] with { ShardId = null }
                    }
                )
            },
        };
        
        sut.IsConsistent(snapshot).Should().BeFalse();
    }
    
    [Fact]
    public void SnapshotWithMissingShardSnapshotFailsValidation()
    {
        var sut = new CampaignSnapshotCrossServerConsistencyValidator();
        CampaignSnapshot snapshot = ConsistentSnapshot with
        {
            Shards = ConsistentSnapshot.Shards.Without(Shard3Id),
        };
        
        sut.IsConsistent(snapshot).Should().BeFalse();
    }
    
    [Fact]
    public void SnapshotWithExtraShardSnapshotFailsValidation()
    {
        var sut = new CampaignSnapshotCrossServerConsistencyValidator();
        var extraShardId = Guid.NewGuid();
        CampaignSnapshot snapshot = ConsistentSnapshot with
        {
            Shards = ConsistentSnapshot.Shards.With(new Dictionary<Guid, ShardSnapshot>
            {
                [extraShardId] = ConsistentSnapshot.Shards[Shard3Id],
            }),
        };
        
        sut.IsConsistent(snapshot).Should().BeFalse();
    }
    
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
            Statuses = ImmutableDictionary<Guid, StatusContextSnapshot>.Empty,
            IsRemoved = false,
            Position = default,
            Azimuth = 0,
        };
    }
    
    private static ProjectileSnapshot CreateProjectileEntitySnapshot(Guid id)
    {
        return new()
        {
            Id = id,
            AbilityContext = new AbilityContextSnapshot
            {
                Ability = Ability.Instance<RangedAttackAbility>(),
                Level = 1,
                UserId = Player1Id,
                UserStats = ImmutableDictionary<Stat, double>.Empty
            },
            Speed = 0,
            Target = Vector2.Zero,
            IsRemoved = false,
            Position = Vector2.Zero,
            Azimuth = 0
        };
    }
}
