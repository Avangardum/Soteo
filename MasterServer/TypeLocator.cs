using System.Collections.Immutable;
using Soteo.MasterServer.Interfaces;
using Soteo.MasterServer.PacketHandlers;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.MasterServer;

public static class TypeLocator
{
    
    public static readonly ImmutableDictionary<PacketType, Type> PacketHandlerTypes;
    
    static TypeLocator()
    {
        PacketHandlerTypes = typeof(IPacketHandler).Assembly.ExportedTypes
            .Where(it => !it.IsAbstract && it.IsAssignableTo(typeof(IPacketHandler)))
            .ToImmutableDictionary(it => it.GetPacketType(typeof(PacketHandler<>)));
    }
}