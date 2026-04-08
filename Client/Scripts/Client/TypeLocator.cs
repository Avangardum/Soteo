using System.Collections.Generic;
using System.Linq;
using Soteo.Client.Interfaces;
using Soteo.Client.PacketHandlers;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Client;

public static class TypeLocator
{
    public static readonly Dictionary<PacketType, Type> PacketHandlerTypes;
    
    static TypeLocator()
    {
        PacketHandlerTypes = typeof(IPacketHandler).Assembly.ExportedTypes
            .Where(it =>
                !it.IsAbstract && it != typeof(RoutingPacketHandler) && it.IsAssignableTo(typeof(IPacketHandler)))
            .ToDictionary(it => it.GetPacketType(typeof(PacketHandler<>)));
    }
}