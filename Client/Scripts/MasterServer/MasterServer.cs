using Microsoft.Extensions.DependencyInjection;
using Soteo.MasterServer.Communicators;
using Soteo.MasterServer.GameState.Repositories;
using Soteo.MasterServer.Interfaces;
using Soteo.MasterServer.PacketHandlers;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.PacketSerializers;

namespace Soteo.MasterServer;

public sealed class MasterServer : Node
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
        
        if (UseJsmq) services.AddSingleton<ICommunicator, JsmqFromMasterServerCommunicator>();
        else services.AddSingleton<ICommunicator, WebSocketFromMasterServerToGameplayCommunicator>();
        
        foreach (Type type in TypeLocator.PacketHandlerTypes.Values) services.AddTransient(type);
    }
}