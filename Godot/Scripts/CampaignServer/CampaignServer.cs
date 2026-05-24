using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Soteo.CampaignServer.Communicators;
using Soteo.Core.CampaignServer;
using Soteo.Core.CampaignServer.GameState.Repositories;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.CampaignServer.PacketHandlers;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.PacketSerializers;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Util;
using IPacketSender = Soteo.Core.CampaignServer.Interfaces.IPacketSender;

namespace Soteo.CampaignServer;

public sealed class CampaignServer : Node
{
    private ICommunicator _communicator = null!;
    
    public override void _Ready()
    {
        ConstInitializer.Init();
        TypeLocator.Init(CoreCampaignServerAssembly.Value, CoreSharedAssembly.Value);
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildAutofacServiceProvider();
        _communicator = serviceProvider.GetRequiredService<ICommunicator>();
    }

    public override void _Process(float delta)
    {
        _communicator.Poll();
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<ICharacterRepository, CharacterRepository>();
        services.AddSingleton<IPacketHandler, RoutingPacketHandler>();
        services.AddSingleton<IPacketSerializer, RoutingPacketSerializer>();
        services.AddAlias<IPacketSender, ICommunicator>();
        
        if (Const.UseJsmq) services.AddSingleton<ICommunicator, JsmqFromCampaignServerCommunicator>();
        else services.AddSingleton<ICommunicator, WebSocketFromCampaignServerToGameplayCommunicator>();
        
        foreach (Type type in TypeLocator.PacketHandlerTypes.Values) services.AddTransient(type);
    }
}