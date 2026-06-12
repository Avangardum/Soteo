using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Attributes;
using Soteo.Core.CampaignServerState.DataObjects;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.CampaignServer;

public sealed class CampaignServerRoutingPacketHandler(IServiceProvider serviceProvider) : IPacketHandler
{
    public async Task HandleAsync(Packet packet, Guid senderId)
    {
        IPacketHandler? handler =
            serviceProvider.GetPacketHandlerFor<CampaignServerPacketHandlerAttribute>(packet.TypeCode);
        if (handler == null)
            throw ExceptionFactory.PacketHandlerNotFound(packet.TypeCode, typeof(CampaignServerPacketHandlerAttribute));
        User sender = serviceProvider.GetRequiredService<IUserRepository>()[senderId];
        if (sender.IsPlayer && !handler.GetType().HasAttribute<AllowClientPacketsAttribute>())
            throw ExceptionFactory.ClientPacketsNotAllowed(handler.GetType());
        await handler.HandleAsync(packet, senderId);
    }
}
