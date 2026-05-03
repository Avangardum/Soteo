using Microsoft.Extensions.DependencyInjection;
using Soteo.CampaignServer.GameState.DataObjects;
using Soteo.CampaignServer.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Packets;

namespace Soteo.CampaignServer.PacketHandlers;

public sealed class RoutingPacketHandler(IServiceProvider serviceProvider) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, User sender)
    {
        if (!TypeLocator.PacketHandlerTypes.TryGetValue(packet.Type, out Type handlerType))
            throw new BadPacketException($"Packet of type {packet.Type} can't be handled");
        if (sender.IsPlayer && !handlerType.HasAttribute<AllowClientPacketsAttribute>())
            throw new BadPacketException($"Clients are not allowed to send packets of type {packet.Type}");
        var handler = (IPacketHandler)serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync(packet, sender);
    }
}