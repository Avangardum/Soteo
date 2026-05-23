using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

public abstract record Packet
{
    protected Packet()
    {
        Type = GetType().GetRequiredAttribute<PacketTypeAttribute>().Type;
    }
    
    public PacketType Type { get; }
}