using Microsoft.Extensions.DependencyInjection;
using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Extensions;
using Soteo.Shared.Packets;

namespace Soteo.MasterServer.PacketHandlers;

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