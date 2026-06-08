using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Soteo.CampaignServer.Communicators;
using Soteo.Core.CampaignServer;
using Soteo.Core.CampaignServer.GameState.Repositories;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.CampaignServer.PacketHandlers;
using Soteo.Core.Gameplay;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;
using Soteo.Core.Shared.PacketSerializers;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Util;
using IPacketSender = Soteo.Core.CampaignServer.Interfaces.IPacketSender;

namespace Soteo.CampaignServer;

public sealed class CampaignServer : Node
{
    private readonly bool _useJsmq = OS.HasFeature("web") && OS.GetCmdlineArgs().Contains("--singleplayer");
    
    private LateInit<ICommunicator> _communicator = new();
    private LateInit<IServiceProvider> _serviceProvider = new();
    
    private IServiceProvider ServiceProvider => _serviceProvider.Value;
    
    public override void _Ready()
    {
        GlobalInit.Init();
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        _serviceProvider.Value = serviceCollection.BuildAutofacServiceProvider();
        _communicator.Value = ServiceProvider.GetRequiredService<ICommunicator>();
        StartShardServers();
        TestLifetimeAsync().CollectException();
    }

    public override void _Process(float delta)
    {
        _communicator.Value.Poll();
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IPlayerCharacterRepository, PlayerCharacterRepository>();
        services.AddSingleton<IPacketHandler, CampaignServerRoutingPacketHandler>();
        services.AddSingleton<IPacketSerializer, RoutingPacketSerializer>();
        services.AddAlias<IPacketSender, ICommunicator>();
        services.AddSingleton<IGameplaySerializer, GameplaySerializer>();
        
        var typeLocator = new TypeLocator(CoreCampaignServerAssembly.Value, CoreSharedAssembly.Value);
        services.AddSingleton<ITypeLocator>(typeLocator);
        
        if (_useJsmq)
            services.AddSingleton<ICommunicator, JsmqFromCampaignServerCommunicator>();
        else
            services.AddSingleton<ICommunicator, WebSocketFromCampaignServerToGameplayCommunicator>();
        
        foreach (Type type in PacketSerializer.AllTypes(typeLocator))
            services.AddSingleton(type);
        
        foreach (Type type in PacketHandler.AllTypes(typeLocator))
            services.AddSingleton(type);
    }
    
    private void StartShardServers()
    {
        // todo redirect output
        ImmutableList<Guid> ids = 
        [
            Guid.Parse("00000000-0000-0000-0000-00000007e571"),
            Guid.Parse("00000000-0000-0000-0000-00000007e572"),
            Guid.Parse("00000000-0000-0000-0000-00000007e573"),
        ];
        
        foreach (Guid id in ids)
        {
            var process = new Process();
            process.StartInfo.FileName = "godot3.6.2.exe";
            process.StartInfo.Arguments = $"--no-window --server {id}";
            process.StartInfo.UseShellExecute = false;
            process.Start();
        }
    }
    
    private async Task TestLifetimeAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(30));
        ServiceProvider.GetRequiredService<IPacketSender>().Broadcast(new PausePacket { Pause = true });
        await Task.Delay(TimeSpan.FromSeconds(10));
        ServiceProvider.GetRequiredService<IPacketSender>().Broadcast(new PausePacket { Pause = false });
    }
}
