using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Exceptions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;
using Soteo.Util.Extensions;

namespace Soteo.Core.CampaignServer.PacketHandlers;

public sealed class RoutingPacketHandler(IServiceProvider serviceProvider) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        if (!TypeLocator.PacketHandlerTypes.TryGetValue(packet.Type, out Type? handlerType))
            throw new BadPacketException(PacketHandlerNotFoundMessage(packet.Type));
        User sender = serviceProvider.GetRequiredService<IUserRepository>()[senderId];
        if (sender.IsPlayer && !handlerType.HasAttribute<AllowClientPacketsAttribute>())
            throw new BadPacketException(ClientPacketsNotAllowedMessage(handlerType));
        var handler = (IPacketHandler)serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync(packet, senderId);
    }
    
    private string PacketHandlerNotFoundMessage(PacketType packetType)
    {
        return $"Packet handler for packet type {packetType} was not found. " +
            $"Make sure the handler exists, is public and its assembly is passed to TypeLocator.Init.";
    }
    
    private string ClientPacketsNotAllowedMessage(Type handlerType)
    {
        return $"{handlerType} does not allow client packets. Add [AllowClientPackets] if it should allow them.";
    }
}
