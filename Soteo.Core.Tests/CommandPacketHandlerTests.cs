using AwesomeAssertions;
using NSubstitute;
using Soteo.Core.Campaign;
using Soteo.Core.Commands;
using Soteo.Core.Entities;
using Soteo.Core.PacketHandlers.Gameplay.Command;
using Soteo.Core.Packets.Commands;

namespace Soteo.Core.Tests;

public sealed class CommandPacketHandlerTests
{
    private readonly Sut _sut;
    private readonly IEntityManager _entityManager;
    private readonly ISynchronizedCampaignStatePuppetRepository _synchronizedCampaignStateRepo;
    private readonly Guid _unitId;
    private readonly Guid _controllingPlayerId;
    private readonly TestUnit _unit;
    private readonly TestCommand _command;

    public CommandPacketHandlerTests()
    {
        _entityManager = Substitute.For<IEntityManager>();
        _synchronizedCampaignStateRepo = Substitute.For<ISynchronizedCampaignStatePuppetRepository>();
        _synchronizedCampaignStateRepo.State.Returns(new SynchronizedCampaignState { IsPaused = false } );
        _sut = new Sut(_entityManager, _synchronizedCampaignStateRepo);
        _unitId = Guid.NewGuid();
        _controllingPlayerId = Guid.NewGuid();
        _unit = new TestUnit(_unitId, _controllingPlayerId);
        _entityManager.Entities.Returns(new Dictionary<Guid, IEntity> { [_unitId] = _unit } );
        _command = new TestCommand();
    }

    [Fact]
    public async Task HandlingPacketFromControllingPlayerSetsUnitCommand()
    {
        var packet = new TestCommandPacket { UnitId = _unitId, Command = _command };
        await _sut.HandleAsync(packet, _controllingPlayerId);
        _unit.GetCommands().Single().Should().Be(packet.Command);
    }

    [Fact]
    public async Task HandlingPacketFromControllingPlayerWhilePausedDoesNotSetUnitCommand()
    {
        _synchronizedCampaignStateRepo.State.Returns(new SynchronizedCampaignState { IsPaused = true });
        var packet = new TestCommandPacket { UnitId = _unitId, Command = _command };
        await _sut.HandleAsync(packet, _controllingPlayerId);
        _unit.GetCommands().Should().BeEmpty();
    }

    [Fact]
    public async Task HandlingPacketFromNonControllingPlayerDoesNotSetsUnitCommand()
    {
        var packet = new TestCommandPacket { UnitId = _unitId, Command = _command };
        await _sut.HandleAsync(packet, Guid.NewGuid());
        _unit.GetCommands().Should().BeEmpty();
    }

    [Fact]
    public async Task HandlingPacketWithNonexistentUnitIdDoesNotSetUnitCommand()
    {
        var packet = new TestCommandPacket { UnitId = Guid.NewGuid(), Command = _command };
        await _sut.HandleAsync(packet, _controllingPlayerId);
        _unit.GetCommands().Should().BeEmpty();
    }

    private class Sut(IEntityManager entityManager, ISynchronizedCampaignStatePuppetRepository pauseRepo) :
        CommandPacketHandler<TestCommandPacket, TestCommand>(entityManager, pauseRepo);

    private record TestCommandPacket : CommandPacket<TestCommand>;

    private record TestCommand : ICommand;
}
