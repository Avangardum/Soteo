using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core;
using Soteo.Core.Attributes;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Core.Services;
using Soteo.Core.Services.PacketHandlers.CampaignServer;
using Soteo.Core.Services.Repositories;
using Soteo.Core.Services.Serializers;
using Soteo.Core.Services.Serializers.PacketSerializers;
using Soteo.Core.StaticHelpers;
using Soteo.Main.CampaignServer.Communicators;
using Soteo.Main.Shared;
using Soteo.Util;
using File = System.IO.File;

namespace Soteo.Main.CampaignServer;

public sealed class CampaignServer : Node
{
    private readonly bool _useJsmq = OS.HasFeature("web") && OS.GetCmdlineArgs().Contains("--singleplayer");
    
    private readonly LateInit<IFromCampaignServerCommunicator> _communicator = new();
    private readonly LateInit<IServiceProvider> _serviceProvider = new();
    
    private IServiceProvider ServiceProvider => _serviceProvider.Value;
    
    public override void _Ready()
    {
        GlobalInit.Init();
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        _serviceProvider.Value = serviceCollection.BuildAutofacServiceProvider();
        _communicator.Value = ServiceProvider.GetRequiredService<IFromCampaignServerCommunicator>();
        TestLifetimeAsync().CollectException();
    }

    public override void _Process(float delta)
    {
        _communicator.Value.Poll();
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IPlayerCharacterTrackerRepository, PlayerCharacterTrackerRepository>();
        services.AddSingleton<IPacketHandler, CampaignServerRoutingPacketHandler>();
        services.AddSingleton<IPacketSerializer, RoutingPacketSerializer>();
        services.AddAlias<IFromCampaignServerPacketSender, IFromCampaignServerCommunicator>();
        services.AddSingleton<ISerializationHelper, SerializationHelper>();
        services.AddSingleton<ITypeLocator>(new TypeLocator(SoteoCoreAssembly.Value));
        services.AddSingleton<CampaignSnapshotManager>();
        services.AddAlias<ICampaignServerPersistencePacketReceiver, CampaignSnapshotManager>();
        services.AddSingleton
        <
            ICampaignSnapshotCrossServerConsistencyValidator,
            CampaignSnapshotCrossServerConsistencyValidator
        >();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ICampaignSnapshotSerializer, CampaignSnapshotSerializer>();
        services.AddSingleton<IShardServerAllowlist>(ShardServerAllowlist.Enabled(CampaignServerCmdLineArgs.ShardIds));
        
        if (_useJsmq)
            services.AddSingleton<IFromCampaignServerCommunicator, JsmqFromCampaignServerCommunicator>();
        else
            services.AddSingleton<IFromCampaignServerCommunicator, WebSocketFromCampaignServerToGameplayCommunicator>();
        
        foreach (Type type in PacketSerializer.AllTypes(new TypeLocator(SoteoCoreAssembly.Value)))
            services.AddSingleton(type);
        
        foreach (Type type in PacketHandlerLocator<CampaignServerPacketHandlerAttribute>.AllTypes(new TypeLocator(SoteoCoreAssembly.Value)))
            services.AddSingleton(type);
    }
    
    private async Task TestLifetimeAsync()
    {
        var persistenceService = ServiceProvider.GetRequiredService<CampaignSnapshotManager>();
        var snapshotSerializer = ServiceProvider.GetRequiredService<ICampaignSnapshotSerializer>();
        var userRepo = ServiceProvider.GetRequiredService<IUserRepository>();
        var communicator = ServiceProvider.GetRequiredService<IFromCampaignServerCommunicator>();

        await userRepo.WaitForUsersToConnectAsync(CampaignServerCmdLineArgs.ShardIds, timeout: 10);
        communicator.AllowPlayerConnections = true;
        
        if (OS.GetCmdlineArgs().Contains("--singleplayer")) return;
        
        if (File.Exists(EnvironmentVariables.CampaignSnapshotPath))
        {
            var bytes = File.ReadAllBytes(EnvironmentVariables.CampaignSnapshotPath);
            var snapshot = snapshotSerializer.Deserialize(bytes);
            await persistenceService.ReplicateSnapshotAsync(snapshot);
        }
        
        await Task.Delay(TimeSpan.FromSeconds(15));
        var packetSender = ServiceProvider.GetRequiredService<IFromCampaignServerPacketSender>();
        packetSender.BroadcastToAll(new PausePacket { Pause = false });
        await Task.Delay(TimeSpan.FromSeconds(15));
        packetSender.BroadcastToAll(new PausePacket { Pause = true });

        {
            CampaignSnapshot snapshot = await persistenceService.CreateSnapshotAsync();
            var bytes = snapshotSerializer.Serialize(snapshot);
            File.WriteAllBytes(EnvironmentVariables.CampaignSnapshotPath, bytes);
        }
    }
}
