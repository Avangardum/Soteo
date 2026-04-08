using Soteo.MasterServer.Interfaces;
using Soteo.MasterServer.PacketHandlers;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.MasterServer;

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