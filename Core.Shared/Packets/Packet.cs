using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Util.Extensions;

namespace Soteo.Core.Shared.Packets;

public abstract record Packet // todo use required fields, avoid gradual construction
{
    protected Packet()
    {
        Type = GetType().GetRequiredAttribute<PacketTypeAttribute>().Type;
    }
    
    public PacketType Type { get; }
}
