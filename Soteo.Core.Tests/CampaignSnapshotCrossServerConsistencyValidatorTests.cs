// using System.Collections.Immutable;
// using Soteo.Core.CampaignServer.Dto.Snapshots;
// using Soteo.Core.Shared.Dto.Snapshots;
//
// namespace Soteo.Core.CampaignServer.Tests;
//
// public sealed class CampaignSnapshotCrossServerConsistencyValidatorTests
// {
//     private static readonly Guid Player1Id = Guid.NewGuid(); 
//     private static readonly Guid Player2Id = Guid.NewGuid(); 
//     private static readonly Guid Player3Id = Guid.NewGuid();
//     private static readonly Guid Char1Id = Guid.NewGuid(); 
//     private static readonly Guid Char2Id = Guid.NewGuid(); 
//     private static readonly Guid Char3Id = Guid.NewGuid();
//     private static readonly Guid Shard1Id = Guid.NewGuid(); 
//     private static readonly Guid Shard2Id = Guid.NewGuid(); 
//     private static readonly Guid Shard3Id = Guid.NewGuid(); 
//     
//     private static readonly CampaignSnapshot ConsistentSnapshot = new()
//     {
//         CampaignServer = new CampaignServerSnapshot
//         {
//             Users = new Dictionary<Guid, UserSnapshot>
//             {
//                 [Player1Id] = new()
//                 {
//                     Id = Player1Id,
//                     IsConnected = true,
//                     IsPlayer = true,
//                     IsShard = false,
//                 },
//                 [Player2Id] = new()
//                 {
//                     Id = Player2Id,
//                     IsConnected = true,
//                     IsPlayer = true,
//                     IsShard = false,
//                 },
//                 [Player3Id] = new()
//                 {
//                     Id = Player3Id,
//                     IsConnected = true,
//                     IsPlayer = true,
//                     IsShard = false,
//                 },
//                 [Shard1Id] = new()
//                 {
//                     Id = Shard1Id,
//                     IsConnected = true,
//                     IsPlayer = false,
//                     IsShard = true,
//                 },
//                 [Shard2Id] = new()
//                 {
//                     Id = Shard2Id,
//                     IsConnected = true,
//                     IsPlayer = false,
//                     IsShard = true,
//                 },
//                 [Shard3Id] = new()
//                 {
//                     Id = Shard3Id,
//                     IsConnected = true,
//                     IsPlayer = false,
//                     IsShard = true,
//                 },
//             }.ToImmutableDictionary(),
//             Characters = new Dictionary<Guid, PlayerCharacterSnapshot>
//             {
//                 [Char1Id] = new()
//                 {
//                     Id = Char1Id,
//                     PlayerId = Player1Id,
//                     ShardId = Shard1Id,
//                 },
//                 [Char2Id] = new()
//                 {
//                     Id = Char2Id,
//                     PlayerId = Player2Id,
//                     ShardId = Shard2Id,
//                 },
//                 [Char3Id] = new()
//                 {
//                     Id = Char3Id,
//                     PlayerId = Player3Id,
//                     ShardId = null,
//                 },
//             }.ToImmutableDictionary(),
//         },
//         Shards = new Dictionary<Guid, ShardSnapshot>
//         {
//             [Shard1Id] = new()
//             {
//                 Tick = 0,
//                 Entities = new Dictionary<Guid, EntitySnapshot>
//                 {
//                     [Player1Id] = TODO,
//                 }.ToImmutableDictionary(),
//             },
//             [Shard2Id] = new()
//             {
//                 Tick = 0,
//                 Entities = new Dictionary<Guid, EntitySnapshot>
//                 {
//                     [Player2Id] = TODO,
//                 }.ToImmutableDictionary(),
//             },
//             [Shard3Id] = new()
//             {
//                 Tick = 0,
//                 Entities = ImmutableDictionary<Guid, EntitySnapshot>.Empty,
//             },
//         }.ToImmutableDictionary(),
//     };
//     
//     [Fact]
//     public void ConsistentSnapshotPassesValidation()
//     {
//         
//     }
// }
