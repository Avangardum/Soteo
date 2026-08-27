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

public sealed class SynchronizationServerTests
{
    private readonly SynchronizationServer _sut;
    private readonly CurrentTickRepository _tickRepo;
    private readonly FakePauseRepo _pauseRepo;
    private readonly FakeFromGameplayPacketSender _packetSender;
    
    public SynchronizationServerTests()
    {
        var entitySnapshotManager = Substitute.For<IEntitySnapshotManager>();
        _packetSender = new FakeFromGameplayPacketSender();
        var connectionNotifier = Substitute.For<IConnectionNotifier>();
        var processPublisher = Substitute.For<IProcessPublisher>();
        var frameStopwatch = Substitute.For<IFrameStopwatch>();
        _pauseRepo = new FakePauseRepo();
        _tickRepo = new CurrentTickRepository();
        var initRepo = new FakeInitializationRepo { Initialized = true };
        _sut = new SynchronizationServer
        (
            entitySnapshotManager,
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
        _tickRepo.Value = 100;
        
        var playerId = Guid.NewGuid();
        _sut.ReceiveSnapshotRequest(playerId);
        _sut.Tick(Const.TickInterval);
        
        var snapshotPacket = _packetSender.PacketsSentTo(playerId).OfType<ShardSnapshotPacket>().Single();
        snapshotPacket.Snapshot.Tick.Should().Be(100);
    }
    
    // todo replicate create char when paused bug
}
