using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Extensions;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public sealed class RoutingPacketHandler
(
    IServiceProvider rootServiceProvider,
    IShardServiceProvider shardServiceProvider,
    ICurrentUserIdRepository currentUserIdRepository,
    IPacketSender packetSender
) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        IServiceProvider? serviceProvider =
            IsServer ? shardServiceProvider.GetServiceProviderForShard(currentUserIdRepository.UserId) :
            senderId == MasterServerId ? rootServiceProvider :
            shardServiceProvider.GetServiceProviderForShard(senderId);
        if (serviceProvider == null) return;

        if (!TypeLocator.PacketHandlerTypes.TryGetValue(packet.Type, out Type handlerType))
            throw new BadPacketException($"Packet of type {packet.Type} can't be handled");
        if (IsServer && senderId != MasterServerId && !handlerType.HasAttribute<AllowClientPacketsAttribute>())
            throw new BadPacketException($"Clients are not allowed to send packets of type {packet.Type}");
        var handler = (IPacketHandler)serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync(packet, senderId);
    }
}