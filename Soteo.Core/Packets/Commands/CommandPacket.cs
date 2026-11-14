using Soteo.Core.Commands;

namespace Soteo.Core.Packets.Commands;

public abstract record CommandPacket<T> : Packet where T : Command
{
    public required Guid UnitId { get; init; }
    public required T Command { get; init; }
}
