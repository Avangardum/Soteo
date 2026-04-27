using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.PacketHandlers;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Util;

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