using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Soteo.Core;
using Soteo.Core.Attributes;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Options;
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
using Path = System.IO.Path;

namespace Soteo.Main.CampaignServer;

public sealed class CampaignServerMain : Node, ICampaignServerInitPacketReceiver
{
    private readonly Dictionary<Guid, TaskCompletionSource> _shardServerInitAwaitingCampaignServerInitTcs = new();
    
    private readonly bool _useJsmq = OS.HasFeature("web") && Config.IsSingleplayer;
    
    private readonly LateInit<IFromCampaignServerCommunicator> _communicator = new();
    private readonly LateInit<IServiceProvider> _serviceProvider = new();
    
    private IProcessPublisher? _processPublisher;
    
    private IServiceProvider ServiceProvider => _serviceProvider.Value;
    
    public override async void _Ready()
    {
        // todo refactor
        try
        {
            GlobalInit.Init();
            var serviceCollection = new ServiceCollection();
            RegisterServices(serviceCollection);
            CreateSingletonNodes();
            _serviceProvider.Value = serviceCollection.BuildAutofacServiceProvider();
            _communicator.Value = ServiceProvider.GetRequiredService<IFromCampaignServerCommunicator>();

            var snapshotManager = ServiceProvider.GetRequiredService<CampaignSnapshotManager>();
            var snapshotSerializer = ServiceProvider.GetRequiredService<ICampaignSnapshotSerializer>();
            var userRepo = ServiceProvider.GetRequiredService<IUserRepository>();
            var communicator = ServiceProvider.GetRequiredService<IFromCampaignServerCommunicator>();
            var synchronizedCampaignStateRepo =
                ServiceProvider.GetRequiredService<ISynchronizedCampaignStateRepository>();
            var timeProvider = ServiceProvider.GetRequiredService<TimeProvider>();
            // todo unwrap
            var campaignPersistenceOptions = ServiceProvider.GetRequiredService<IOptions<CampaignPersistenceOptions>>();
            IReadOnlyList<Guid> shardIds = ServiceProvider.GetRequiredService<CampaignOptions>().ShardIds;
            bool isSingleplayer = ServiceProvider.GetRequiredService<SingleplayerOptions>().IsSingleplayer;

            await userRepo.WaitForUsersToConnectAsync(shardIds, timeout: 30);

            // Create a task for each shard server waiting for it to send
            // ShardServerInitAwaitingCampaignServerInitPacket, notifying that all initializing steps are done,
            // except for waiting for other servers' initialization. Once all the shard servers sent that, we can
            // tell them to complete initialization.
            foreach (Guid id in shardIds)
                _shardServerInitAwaitingCampaignServerInitTcs[id] = new TaskCompletionSource();
            
            Func<string> snapshotPath = () => Path.Combine(campaignPersistenceOptions.Value.SnapshotFolder, "Snapshot");
            if (!isSingleplayer && File.Exists(snapshotPath()))
            {
                byte[] bytes = File.ReadAllBytes(snapshotPath());
                CampaignSnapshot snapshot = snapshotSerializer.Deserialize(bytes);
                await snapshotManager.ReplicateSnapshotAsync(snapshot);
            }
            else
            {
                communicator.BroadcastToShardServers(new NoInitialShardSnapshotPacket());
            }

            // todo timeout
            await Task.WhenAll(_shardServerInitAwaitingCampaignServerInitTcs.Values.Select(it => it.Task));
            communicator.BroadcastToShardServers(new CampaignInitializedPacket());
            communicator.AllowPlayerConnections = true; // todo check init state inside a communicator

            await timeProvider.Delay(TimeSpan.FromSeconds(15));
            synchronizedCampaignStateRepo.Value = synchronizedCampaignStateRepo.Value with { IsPaused = false };
            await timeProvider.Delay(TimeSpan.FromSeconds(15));
            synchronizedCampaignStateRepo.Value = synchronizedCampaignStateRepo.Value with { IsPaused = true };

            if (!isSingleplayer)
            {
                CampaignSnapshot snapshot = await snapshotManager.CreateSnapshotAsync();
                byte[] bytes = snapshotSerializer.Serialize(snapshot);
                File.WriteAllBytes(snapshotPath(), bytes);
            }
        }
        catch (Exception e)
        {
            AsyncExceptionCollector.Collect(e);
        }
    }

    public override void _Process(float delta)
    {
        _communicator.Value.Poll();
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICampaignServerInitPacketReceiver>(this);
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
        services.AddSingleton<IShardServerAllowlist>(sp =>
            ShardServerAllowlist.Enabled(sp.GetRequiredService<CampaignOptions>().ShardIds));
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
        
        Config.RegisterConfigurationOptions(services);
    }
    
    private void CreateSingletonNodes()
    {
        _processPublisher = new ProcessPublisher().Also(it => AddChild(it));
    }

    public void ReceiveShardServerInitAwaitingCampaignServerInitPacket(Guid senderId)
    {
        _shardServerInitAwaitingCampaignServerInitTcs[senderId].SetResult();
    }
}
