using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Interfaces;

namespace Soteo.Core.Shared.Extensions;

public static class ServiceProviderExtensions
{
    extension (IServiceProvider self)
    {
        public IPacketHandler? GetPacketHandlerFor(PacketTypeCode packetTypeCode)
        {
            var typeLocator = self.GetRequiredService<ITypeLocator>();
            return (IPacketHandler?)PacketHandler.TypeFor(packetTypeCode, typeLocator)?.PassTo(self.GetRequiredService);
        }
    }
}
