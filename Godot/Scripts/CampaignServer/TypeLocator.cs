using Soteo.CampaignServer.Interfaces;
using Soteo.CampaignServer.PacketHandlers;
using Soteo.Shared.Enums;

namespace Soteo.CampaignServer;

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