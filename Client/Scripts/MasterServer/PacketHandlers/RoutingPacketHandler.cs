using Microsoft.Extensions.DependencyInjection;
using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Packets.Shared;

namespace Soteo.MasterServer.PacketHandlers;

public sealed class RoutingPacketHandler(IServiceProvider serviceProvider) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, User sender)
    {
        if (!TypeLocator.PacketHandlerTypes.TryGetValue(packet.Type, out Type handlerType))
            throw new BadPacketException($"Packet of type {packet.Type} can't be handled");
        var handler = (IPacketHandler)serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync(packet, sender);
    }
}