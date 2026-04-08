using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Client.Interfaces;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Client.PacketHandlers;

public sealed class RoutingPacketHandler
(
    IServiceProvider rootServiceProvider,
    IShardServiceProvider shardServiceProvider,
    IUserIdRepository userIdRepository,
    IPacketSender packetSender
) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        IServiceProvider? serviceProvider =
            IsServer ? shardServiceProvider.GetServiceProviderForShard(userIdRepository.UserId) :
            senderId == MasterServerId ? rootServiceProvider :
            shardServiceProvider.GetServiceProviderForShard(senderId);
        if (serviceProvider == null) return;

        try
        {
            if (!TypeLocator.PacketHandlerTypes.TryGetValue(packet.Type, out Type handlerType))
                throw new BadPacketException($"Packet of type {packet.Type} can't be handled");
            var handler = (IPacketHandler)serviceProvider.GetRequiredService(handlerType);
            await handler.HandleAsync(packet, senderId);
        }
        catch (BadPacketException e)
        {
            if (IsServer) packetSender.SendReliable(new BadInputPacket { Reason = e.Reason }, senderId);
            else throw;
        }
    }
}