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
    IShardServiceProviderSource shardServiceProviderSource,
    ICurrentUserIdRepository currentUserIdRepository
) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        IServiceProvider? serviceProvider =
            IsServer ? shardServiceProviderSource.ShardServiceProviders[currentUserIdRepository.UserId] :
            senderId == MasterServerId ? rootServiceProvider :
            shardServiceProviderSource.ShardServiceProviders[senderId];
        if (serviceProvider == null) return;

        if (!TypeLocator.PacketHandlerTypes.TryGetValue(packet.Type, out Type handlerType))
            throw new BadPacketException($"Packet of type {packet.Type} can't be handled");
        if (IsServer && senderId != MasterServerId && !handlerType.HasAttribute<AllowClientPacketsAttribute>())
            throw new BadPacketException($"Clients are not allowed to send packets of type {packet.Type}");
        var handler = (IPacketHandler)serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync(packet, senderId);
    }
}