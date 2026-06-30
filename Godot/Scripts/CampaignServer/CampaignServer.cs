using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core;
using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;
using Soteo.Core.Services;
using Soteo.Core.Services.PacketHandlers.CampaignServer;
using Soteo.Core.Services.Repositories;
using Soteo.Core.Services.Serializers;
using Soteo.Core.Services.Serializers.Packet;
using Soteo.Core.StaticHelpers;
using Soteo.Main.CampaignServer.Communicators;
using Soteo.Main.Shared;
using Soteo.Util;

namespace Soteo.Main.CampaignServer;

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
        services.AddSingleton<IPlayerCharacterRepository, PlayerCharacterTrackerRepository>();
        services.AddSingleton<IPacketHandler, CampaignServerRoutingPacketHandler>();
        services.AddSingleton<IPacketSerializer, RoutingPacketSerializer>();
        services.AddAlias<IFromCampaignServerPacketSender, ICommunicator>();
        services.AddSingleton<ISerializationHelper, SerializationHelper>();
        
        var typeLocator = new TypeLocator(SoteoCoreAssembly.Value);
        services.AddSingleton<ITypeLocator>(typeLocator);
        
        if (_useJsmq)
            services.AddSingleton<ICommunicator, JsmqFromCampaignServerCommunicator>();
        else
            services.AddSingleton<ICommunicator, WebSocketFromCampaignServerToGameplayCommunicator>();
        
        foreach (Type type in PacketSerializer.AllTypes(typeLocator))
            services.AddSingleton(type);
        
        foreach (Type type in PacketHandlerLocator<CampaignServerPacketHandlerAttribute>.AllTypes(typeLocator))
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
        ServiceProvider.GetRequiredService<IFromCampaignServerPacketSender>().BroadcastToShardServersAndClients(new PausePacket { Pause = true });
        await Task.Delay(TimeSpan.FromSeconds(10));
        ServiceProvider.GetRequiredService<IFromCampaignServerPacketSender>().BroadcastToShardServersAndClients(new PausePacket { Pause = false });
    }
}
