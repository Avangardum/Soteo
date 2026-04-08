using Microsoft.Extensions.DependencyInjection;
using Soteo.MasterServer.GameState.Repositories;
using Soteo.MasterServer.Interfaces;
using Soteo.MasterServer.PacketHandlers;
using Soteo.Shared;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.PacketSerializers.Shared;

namespace Soteo.MasterServer;

public sealed class MasterServer : Node
{
    private WebSocketCommunicator _communicator = null!;
    
    public override void _Ready()
    {
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        var serviceProvider = new SimpleServiceProvider(serviceCollection);
        _communicator = serviceProvider.GetRequiredService<WebSocketCommunicator>();
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
        services.AddSingleton<IPacketSerializer, UniversalPacketSerializer>();
        services.AddSingleton<WebSocketCommunicator>();
        services.AddAlias<IPacketSender, WebSocketCommunicator>();
        
        foreach (Type type in TypeLocator.PacketHandlerTypes.Values) services.AddTransient(type);
    }
}