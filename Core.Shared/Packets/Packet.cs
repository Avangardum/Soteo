using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Util.Extensions;

namespace Soteo.Core.Shared.Packets;

public abstract record Packet
{
    public PacketTypeCode TypeCode => GetType().GetRequiredAttribute<PacketTypeCodeAttribute>().TypeCode;
}
