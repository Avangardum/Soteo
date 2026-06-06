using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Soteo.CampaignServer.Communicators;
using Soteo.Core.CampaignServer;
using Soteo.Core.CampaignServer.GameState.Repositories;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.CampaignServer.PacketHandlers;
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
    private LateInit<ICommunicator> _communicator = new();
    private LateInit<IServiceProvider> _serviceProvider = new();
    
    private IServiceProvider ServiceProvider => _serviceProvider.Value;
    
    public override void _Ready()
    {
        ConstInitializer.Init();
        TypeLocator.Init(CoreCampaignServerAssembly.Value, CoreSharedAssembly.Value);
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
        services.AddSingleton<ICharacterRepository, CharacterRepository>();
        services.AddSingleton<IPacketHandler, CampaignServerRoutingPacketHandler>();
        services.AddSingleton<IPacketSerializer, RoutingPacketSerializer>();
        services.AddAlias<IPacketSender, ICommunicator>();
        
        if (Const.UseJsmq)
            services.AddSingleton<ICommunicator, JsmqFromCampaignServerCommunicator>();
        else
            services.AddSingleton<ICommunicator, WebSocketFromCampaignServerToGameplayCommunicator>();
        
        foreach (Type type in PacketHandler.TypesByPacketType.Values)
            services.AddTransient(type);
    }
    
    private void StartShardServers()
    {
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
