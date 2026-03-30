using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Shared.Packets.Shared;

public abstract record Packet
{
    protected Packet()
    {
        Type = GetType().GetRequiredAttribute<PacketTypeAttribute>().Type;
        CorrelationId = Guid.NewGuid();
    }
    
    public PacketType Type { get; }
    public Guid CorrelationId { get; set; }
}