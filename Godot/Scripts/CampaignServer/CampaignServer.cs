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
using Soteo.Main.Shared.Nodes;
using Soteo.Util;
using File = System.IO.File;

namespace Soteo.Main.CampaignServer;

public sealed class CampaignServer : Node
{
    private readonly bool _useJsmq = OS.HasFeature("web") && CampaignServerCmdLineArgs.IsSingleplayer;
    
    private readonly LateInit<IFromCampaignServerCommunicator> _communicator = new();
    private readonly LateInit<IServiceProvider> _serviceProvider = new();
    
    private IProcessPublisher? _processPublisher;
    
    private IServiceProvider ServiceProvider => _serviceProvider.Value;
    
    public override void _Ready()
    {
        GlobalInit.Init();
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        CreateSingletonNodes();
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
        services.AddAlias<IConnectionNotifier, IFromCampaignServerCommunicator>();
        services.AddSingleton<ISerializationHelper, SerializationHelper>();
        services.AddSingleton<ITypeLocator>(new TypeLocator(SoteoCoreAssembly.Value));
        services.AddSingleton<CampaignSnapshotManager>();
        services.AddAlias<ICampaignServerPersistencePacketReceiver, CampaignSnapshotManager>();
        services.AddSingleton
        <
            ICampaignSnapshotCrossServerConsistencyValidator,
            CampaignSnapshotCrossServerConsistencyValidator
        >();
        services.AddSingleton<TimeProvider>(new GodotTimeProvider(GetTree()));
        services.AddSingleton<ICampaignSnapshotSerializer, CampaignSnapshotSerializer>();
        services.AddSingleton<IShardServerAllowlist>(ShardServerAllowlist.Enabled(CampaignServerCmdLineArgs.ShardIds));
        services.AddSingleton<ISynchronizedCampaignStateRepository, SynchronizedCampaignStateRepository>();
        services.AddSingleton<IProcessPublisher>(_ => _processPublisher.Required);
        
        if (_useJsmq)
            services.AddSingleton<IFromCampaignServerCommunicator, JsmqFromCampaignServerCommunicator>();
        else
            services.AddSingleton<IFromCampaignServerCommunicator, WebSocketFromCampaignServerToGameplayCommunicator>();
        
        foreach (Type type in PacketSerializer.AllTypes(new TypeLocator(SoteoCoreAssembly.Value)))
            services.AddSingleton(type);
        
        foreach (Type type in PacketHandlerLocator<CampaignServerPacketHandlerAttribute>.AllTypes(new TypeLocator(SoteoCoreAssembly.Value)))
            services.AddSingleton(type);
    }
    
    private void CreateSingletonNodes()
    {
        _processPublisher = new ProcessPublisher().Also(it => AddChild(it));
    }
    
    private async Task TestLifetimeAsync()
    {
        var snapshotManager = ServiceProvider.GetRequiredService<CampaignSnapshotManager>();
        var snapshotSerializer = ServiceProvider.GetRequiredService<ICampaignSnapshotSerializer>();
        var userRepo = ServiceProvider.GetRequiredService<IUserRepository>();
        var communicator = ServiceProvider.GetRequiredService<IFromCampaignServerCommunicator>();
        var synchronizedCampaignStateRepo = ServiceProvider.GetRequiredService<ISynchronizedCampaignStateRepository>();
        var timeProvider = ServiceProvider.GetRequiredService<TimeProvider>();

        await userRepo.WaitForUsersToConnectAsync(CampaignServerCmdLineArgs.ShardIds, timeout: 10);
        communicator.AllowPlayerConnections = true;
        
        if (!CampaignServerCmdLineArgs.IsSingleplayer && File.Exists(EnvironmentVariables.CampaignSnapshotPath))
        {
            var bytes = File.ReadAllBytes(EnvironmentVariables.CampaignSnapshotPath);
            var snapshot = snapshotSerializer.Deserialize(bytes);
            await snapshotManager.ReplicateSnapshotAsync(snapshot);
        }
        
        await timeProvider.Delay(TimeSpan.FromSeconds(15));
        synchronizedCampaignStateRepo.Value = synchronizedCampaignStateRepo.Value with { IsPaused = false };
        await timeProvider.Delay(TimeSpan.FromSeconds(15));
        synchronizedCampaignStateRepo.Value = synchronizedCampaignStateRepo.Value with { IsPaused = true };

        if (!CampaignServerCmdLineArgs.IsSingleplayer)
        {
            CampaignSnapshot snapshot = await snapshotManager.CreateSnapshotAsync();
            byte[] bytes = snapshotSerializer.Serialize(snapshot);
            File.WriteAllBytes(EnvironmentVariables.CampaignSnapshotPath, bytes);
        }
    }
}
