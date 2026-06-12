using Soteo.Core.Enums;
using Soteo.Core.Exceptions;

namespace Soteo.Core;

public static class ExceptionFactory
{
    public static Exception TypeNotFound(Type type)
    {
        var message = $"{type} was not found. Make sure it's public and its assembly is passed to TypeLocator.Init.";
        return new Exception(message);
    }
    
    public static BadPacketException PacketHandlerNotFound(PacketTypeCode packetTypeCode, Type requiredAttributeType)
    {
        var message = $"Packet handler for packet type {packetTypeCode} was not found. " +
            $"Make sure the handler exists, is public, decorated with {requiredAttributeType.Name} " +
            $"and its assembly is passed to TypeLocator.Init.";
        return new BadPacketException(message);
    }
    
    public static BadPacketException ClientPacketsNotAllowed(Type handlerType)
    {
        var message = $"{handlerType} does not allow client packets. Add [AllowClientPackets] if it should allow them.";
        return new BadPacketException(message);
    }
}
