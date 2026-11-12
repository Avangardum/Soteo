using Soteo.Core.Commands;

namespace Soteo.Core.Packets.Command;

public abstract record CommandPacket<T> : Packet where T : ICommand
{
    public required Guid UnitId { get; init; }
    public required T Command { get; init; }
}
