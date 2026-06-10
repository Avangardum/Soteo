using NSubstitute;
using Soteo.Core.Interfaces;
using Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;
using Soteo.Core.Packets;

namespace Soteo.Core.Tests;

public sealed class CommandPacketHandlerTests
{
    private readonly Sut _sut;
    private readonly IEntityManager _entityManager;
    private readonly IPauseRepository _pauseRepo;
    private readonly Guid _unitId;
    private readonly Guid _controllingPlayerId;
    private readonly ICommandableUnit _unit;
    private readonly TestCommand _command;

    public CommandPacketHandlerTests()
    {
        _entityManager = Substitute.For<IEntityManager>();
        _pauseRepo = Substitute.For<IPauseRepository>();
        _sut = new Sut(_entityManager, _pauseRepo);
        _unitId = Guid.NewGuid();
        _unit = Substitute.For<ICommandableUnit>();
        _unit.Id.Returns(_unitId);
        _controllingPlayerId = Guid.NewGuid();
        _unit.ControllingPlayerIds.Returns(new HashSet<Guid> { _controllingPlayerId }.AsReadOnly());
        _entityManager.Entities.Returns(new Dictionary<Guid, IEntity> { [_unitId] = _unit } );
        _command = new TestCommand();
    }
    
    [Fact]
    public async Task HandlingPacketFromControllingPlayerSetsUnitCommand()
    {
        var packet = new TestCommandPacket { UnitId = _unitId, Command = _command };
        await _sut.HandleAsync(packet, _controllingPlayerId);
        _unit.Received(1).SetCommand(_command);
    }
    
    [Fact]
    public async Task HandlingPacketFromControllingPlayerWhilePausedDoesNotSetUnitCommand()
    {
        _pauseRepo.Paused.Returns(true);
        var packet = new TestCommandPacket { UnitId = _unitId, Command = _command };
        await _sut.HandleAsync(packet, _controllingPlayerId);
        _unit.Received(0).SetCommand(_command);
    }
    
    [Fact]
    public async Task HandlingPacketFromNonControllingPlayerDoesNotSetsUnitCommand()
    {
        var packet = new TestCommandPacket { UnitId = _unitId, Command = _command };
        await _sut.HandleAsync(packet, Guid.NewGuid());
        _unit.Received(0).SetCommand(Arg.Any<ICommand>());
    }
    
    [Fact]
    public async Task HandlingPacketWithNonexistentUnitIdDoesNotSetUnitCommand()
    {
        var packet = new TestCommandPacket { UnitId = Guid.NewGuid(), Command = _command };
        await _sut.HandleAsync(packet, _controllingPlayerId);
        _unit.Received(0).SetCommand(Arg.Any<ICommand>());
    }
    
    private class Sut(IEntityManager entityManager, IPauseRepository pauseRepo) :
        CommandPacketHandler<TestCommandPacket, TestCommand>(entityManager, pauseRepo);

    private record TestCommandPacket : CommandPacket<TestCommand>;
    
    private record TestCommand : ICommand;
}
