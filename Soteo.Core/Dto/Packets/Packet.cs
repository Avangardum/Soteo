using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Packets;

public abstract record Packet
{
    public PacketTypeCode TypeCode => GetType().GetRequiredAttribute<PacketTypeCodeAttribute>().TypeCode;
}
