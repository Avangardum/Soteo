using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core;
using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Dto.Snapshots;
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
using File = System.IO.File;

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
        services.AddSingleton<ITypeLocator>(new TypeLocator(SoteoCoreAssembly.Value));
        services.AddSingleton<PersistenceService>();
        services.AddAlias<IShardSnapshotPacketReceiver, PersistenceService>();
        services.AddSingleton
        <
            ICampaignSnapshotCrossServerConsistencyValidator,
            CampaignSnapshotCrossServerConsistencyValidator
        >();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ICampaignSnapshotSerializer, CampaignSnapshotSerializer>();
        
        if (_useJsmq)
            services.AddSingleton<ICommunicator, JsmqFromCampaignServerCommunicator>();
        else
            services.AddSingleton<ICommunicator, WebSocketFromCampaignServerToGameplayCommunicator>();
        
        foreach (Type type in PacketSerializer.AllTypes(new TypeLocator(SoteoCoreAssembly.Value)))
            services.AddSingleton(type);
        
        foreach (Type type in PacketHandlerLocator<CampaignServerPacketHandlerAttribute>.AllTypes(new TypeLocator(SoteoCoreAssembly.Value)))
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
        
        // TODO if the campaign server crashes, child processes are not terminated and interfere with future runs
    }
    
    private async Task TestLifetimeAsync()
    {
        await Task.Delay(TimeSpan.FromSeconds(30));
        var packetSender = ServiceProvider.GetRequiredService<IFromCampaignServerPacketSender>();
        packetSender.BroadcastToAll(new PausePacket { Pause = true });
        var persistenceService = ServiceProvider.GetRequiredService<PersistenceService>();
        CampaignSnapshot snapshot = await persistenceService.SaveAsync();
        var snapshotSerializer = ServiceProvider.GetRequiredService<ICampaignSnapshotSerializer>();
        var bytes = snapshotSerializer.Serialize(snapshot);
        File.WriteAllBytes("C:/Users/yuryk/TestCampaignSnapshot", bytes);
        // TODO extract paths to env
    }
}
