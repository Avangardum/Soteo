using Microsoft.Extensions.DependencyInjection;
using Soteo.CampaignServer.Communicators;
using Soteo.CampaignServer.GameState.Repositories;
using Soteo.CampaignServer.Interfaces;
using Soteo.CampaignServer.PacketHandlers;
using Soteo.Shared.Interfaces;
using Soteo.Shared.PacketSerializers;

namespace Soteo.CampaignServer;

public sealed class CampaignServer : Node
{
    private ICommunicator _communicator = null!;
    
    public override void _Ready()
    {
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
        
        if (UseJsmq) services.AddSingleton<ICommunicator, JsmqFromCampaignServerCommunicator>();
        else services.AddSingleton<ICommunicator, WebSocketFromCampaignServerToGameplayCommunicator>();
        
        foreach (Type type in TypeLocator.PacketHandlerTypes.Values) services.AddTransient(type);
    }
}