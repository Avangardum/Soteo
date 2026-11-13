namespace Soteo.Core.PacketHandlers.Gameplay;

public sealed class GameplayRoutingPacketHandler
(
    IServiceProvider rootServiceProvider,
    IShardServiceProviders shardServiceProviders,
    ICurrentUserIdRepository currentUserIdRepository,
    SideOptions sideOptions
) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        IServiceProvider? serviceProvider =
            sideOptions.Side == Side.ShardServer ? shardServiceProviders[currentUserIdRepository.Id.Required] :
            senderId == Const.CampaignServerId ? rootServiceProvider :
            shardServiceProviders.GetValueOrDefault(senderId);
        if (serviceProvider == null) return;

        IPacketHandler handler = serviceProvider.GetPacketHandlerFor<GameplayPacketHandlerAttribute>(packet.GetType()) ??
            throw ExceptionFactory.PacketHandlerNotFound(packet.GetType(), typeof(GameplayPacketHandlerAttribute));

        if
        (
            sideOptions.Side == Side.ShardServer &&
            senderId != Const.CampaignServerId &&
            !handler.GetType().HasAttribute<AllowClientPacketsAttribute>()
        )
        {
            throw ExceptionFactory.ClientPacketsNotAllowed(handler.GetType());
        }

        await handler.HandleAsync(packet, senderId);
    }
}
