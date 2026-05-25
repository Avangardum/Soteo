using System.Diagnostics.CodeAnalysis;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Exceptions;

namespace Soteo.Core.Shared;

public static class Throw
{
    [DoesNotReturn]
    public static void PacketHandlerNotFound(PacketType packetType)
    {
        var message = $"Packet handler for packet type {packetType} was not found. " +
            $"Make sure the handler exists, is public and its assembly is passed to TypeLocator.Init.";
        throw new BadPacketException(message);
    }
    
    [DoesNotReturn]
    public static void ClientPacketsNotAllowed(Type handlerType)
    {
        var message = $"{handlerType} does not allow client packets. Add [AllowClientPackets] if it should allow them.";
        throw new BadPacketException(message);
    }
}
