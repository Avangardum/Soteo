using AwesomeAssertions;
using NSubstitute;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;
using Soteo.Core.Services.Repositories;
using Soteo.Core.Services.Synchronization;
using Soteo.Core.StaticHelpers;
using Soteo.TestUtil;

namespace Soteo.Core.Tests;

public sealed class ShardSynchronizationServerTests
{
    private readonly ShardSynchronizationServer _sut;
    private readonly CurrentTickRepository _tickRepo;
    private readonly FakePauseRepo _pauseRepo;
    private readonly FakeFromGameplayPacketSender _packetSender;
    private readonly IEntitySnapshotManager _entitySnapshotManager;
    
    public ShardSynchronizationServerTests()
    {
        var processPublisher = Substitute.For<IProcessPublisher>();
        _tickRepo = new CurrentTickRepository(processPublisher);
        _entitySnapshotManager = Substitute.For<IEntitySnapshotManager>();
        _packetSender = new FakeFromGameplayPacketSender();
        var connectionNotifier = Substitute.For<IConnectionNotifier>();
        var frameStopwatch = Substitute.For<IFrameStopwatch>();
        _pauseRepo = new FakePauseRepo();
        var initRepo = new FakeInitializationRepo { Initialized = true };
        _sut = new ShardSynchronizationServer
        (
            _entitySnapshotManager,
            _packetSender,
            connectionNotifier,
            processPublisher,
            frameStopwatch,
            _pauseRepo,
            _tickRepo,
            initRepo
        );
    }
    
    [Fact]
    public void SendsFreshSnapshotOnRequestWhenUnpaused()
    {
        // Arrange
        _tickRepo.Tick = 100;
        _pauseRepo.IsPaused = false;
        
        // Act
        
        var player1Id = Guid.NewGuid();
        _sut.ReceiveSnapshotRequest(player1Id);
        _sut.Tick();
        
        _tickRepo.Tick = 101;
        var player2Id = Guid.NewGuid();
        _sut.ReceiveSnapshotRequest(player2Id);
        _sut.Tick();

        _packetSender.PacketsSentTo(player1Id).OfType<ShardSnapshotPacket>().Single().Snapshot.Tick.Should().Be(100);
        _packetSender.PacketsSentTo(player2Id).OfType<ShardSnapshotPacket>().Single().Snapshot.Tick.Should().Be(101);
    }
    
    [Fact]
    public void SendsCachedPreviousSnapshotOnRequestWhenPaused()
    {
        // Arrange
        _tickRepo.Tick = 100;
        _pauseRepo.IsPaused = false;
        
        // Since snapshots are identified by their tick number, it's important that only one snapshot is created
        // per tick number. When the game is paused, the SUT continues ticking despite the tick number
        // not incrementing. While it's possible that the game state changes while paused, different
        // snapshots for the same tick number reflecting these changes should not be created. Instead,
        // the first and only snapshot for this tick number should be cached and reused.
        // This guard detects if a new snapshot is created twice per tick number and throws.
        HashSet<long> ticksWhereEntityPuppetSnapshotsWereCreated = [];
        _entitySnapshotManager
            .When(it => it.CreateEntityPuppetSnapshots())
            .Do(_ =>
            {
                if (!ticksWhereEntityPuppetSnapshotsWereCreated.Add(_tickRepo.Tick))
                    throw new InvalidOperationException("CreateEntityPuppetSnapshots was called twice per tick");
            });
        
        // Act
        
        var player1Id = Guid.NewGuid();
        _sut.ReceiveSnapshotRequest(player1Id);
        _sut.Tick();
        
        _tickRepo.Tick = 101;
        _pauseRepo.IsPaused = true;
        
        var player2Id = Guid.NewGuid();
        _sut.ReceiveSnapshotRequest(player2Id);
        _sut.Tick();
        
        _tickRepo.Tick = 101;
        
        var player3Id = Guid.NewGuid();
        _sut.ReceiveSnapshotRequest(player3Id);
        _sut.Tick();
        
        // Assert
        var player1Packet = _packetSender.PacketsSentTo(player1Id).OfType<ShardSnapshotPacket>().Single();
        var player2Packet = _packetSender.PacketsSentTo(player2Id).OfType<ShardSnapshotPacket>().Single();
        var player3Packet = _packetSender.PacketsSentTo(player3Id).OfType<ShardSnapshotPacket>().Single();
        var theOnePacket = new [] { player1Packet, player2Packet, player3Packet }.Distinct().Single();
        theOnePacket.Snapshot.Tick.Should().Be(100);
    }
    
    [Fact]
    public void SendsNewlyCreatedSnapshotWithPreviousTickNumberOnFirstRequestThenReusesItWhenPausedFromStart()
    {
        // Arrange
        _tickRepo.Tick = 100;
        _pauseRepo.IsPaused = true;
        
        // Act
        
        var player1Id = Guid.NewGuid();
        _sut.ReceiveSnapshotRequest(player1Id);
        _sut.Tick();
        
        var player2Id = Guid.NewGuid();
        _sut.ReceiveSnapshotRequest(player2Id);
        _sut.Tick();
        
        // Assert
        var player1Packet = _packetSender.PacketsSentTo(player1Id).OfType<ShardSnapshotPacket>().Single();
        var player2Packet = _packetSender.PacketsSentTo(player2Id).OfType<ShardSnapshotPacket>().Single();
        var theOnePacket = new [] { player1Packet, player2Packet }.Distinct().Single();
        theOnePacket.Snapshot.Tick.Should().Be(99);
        // Once again, ensure that only one snapshot per tick number 99 is created
        _entitySnapshotManager.Received(1).CreateEntityPuppetSnapshots();
    }
}
