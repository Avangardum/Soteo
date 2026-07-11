using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.Services.Serializers.PacketSerializers;
using Soteo.Core.StaticHelpers;

namespace Soteo.Core.Extensions;

public static class ServiceProviderExtensions
{
    extension (IServiceProvider self)
    {
        public IPacketHandler? GetPacketHandlerFor<TAttribute>(PacketTypeCode packetTypeCode)
            where TAttribute : Attribute
        {
            var typeLocator = self.GetRequiredService<ITypeLocator>();
            return (IPacketHandler?)PacketHandlerLocator<TAttribute>.TypeFor(packetTypeCode, typeLocator)
                ?.PassTo(self.GetRequiredService);
        }
        
        public IPacketSerializer? GetPacketSerializerFor(PacketTypeCode packetTypeCode)
        {
            var typeLocator = self.GetRequiredService<ITypeLocator>();
            return (IPacketSerializer?)PacketSerializer.TypeFor(packetTypeCode, typeLocator)
                ?.PassTo(self.GetRequiredService);
        }
    }
}
