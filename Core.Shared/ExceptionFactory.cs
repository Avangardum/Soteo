using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Exceptions;

namespace Soteo.Core.Shared;

public static class ExceptionFactory
{
    public static Exception TypeNotFound(Type type)
    {
        var message = $"{type} was not found. Make sure it's public and its assembly is passed to TypeLocator.Init.";
        return new Exception(message);
    }
    
    public static Exception PacketHandlerNotFound(PacketType packetType)
    {
        var message = $"Packet handler for packet type {packetType} was not found. " +
            $"Make sure the handler exists, is public and its assembly is passed to TypeLocator.Init.";
        return new BadPacketException(message);
    }
    
    public static Exception ClientPacketsNotAllowed(Type handlerType)
    {
        var message = $"{handlerType} does not allow client packets. Add [AllowClientPackets] if it should allow them.";
        return new BadPacketException(message);
    }
}
