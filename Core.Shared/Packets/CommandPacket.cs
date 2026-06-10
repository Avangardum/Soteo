using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

public abstract record CommandPacket<T> : Packet where T : ICommand
{
    public required Guid UnitId { get; init; }
    public required T Command { get; init; }
}
