using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    private readonly Dictionary<Guid, TaskCompletionSource> _shardServerLocalInitDoneTcs = new();
    
    private readonly bool _useJsmq = OS.HasFeature("web") && Config.IsSingleplayer;
    
    private readonly LateInit<IFromCampaignServerCommunicator> _communicator = new();
    private readonly LateInit<IServiceProvider> _serviceProvider = new();
    private readonly LateInit<CampaignSnapshotManager> _snapshotManager = new();
    private readonly LateInit<ICampaignSnapshotSerializer> _snapshotSerializer = new();
    private readonly LateInit<IUserRepository> _userRepo = new();
    private readonly LateInit<ISynchronizedCampaignStateRepository> _synchronizedCampaignStateRepo = new();
    private readonly LateInit<TimeProvider> _timeProvider = new();
    private readonly LateInit<ILogger<CampaignServerMain>> _logger = new();
    private readonly LateInit<IInitializationRepository> _initRepo = new();
    
    private IProcessPublisher? _processPublisher;
    
    private IServiceProvider ServiceProvider => _serviceProvider.Value;
    private bool IsSingleplayer => ServiceProvider.GetRequiredService<SingleplayerOptions>().IsSingleplayer;
    
    public override void _Ready()
    {
        InitAsync().CollectException();
    }
    
    private async Task InitAsync()
    {
        GlobalInit.Init();
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        CreateSingletonNodes();
        _serviceProvider.Value = serviceCollection.BuildAutofacServiceProvider();
        CreateSingletonServices();
        
        IReadOnlyList<Guid> shardIds = ServiceProvider.GetRequiredService<CampaignOptions>().ShardIds;

        _logger.Value.LogInformation("Waiting for shard servers to connect");
        await _userRepo.Value.WaitForUsersToConnectAsync(shardIds, timeout: 30);

        // Create a task for each shard server waiting for it to finish local initializing, which is everything
        // except for waiting for other servers' initialization. Once all the shard servers sent that, we can
        // tell them to complete initialization.
        foreach (Guid id in shardIds)
            _shardServerLocalInitDoneTcs[id] = new TaskCompletionSource();
        
        await TryLoadSnapshotAsync();
        
        await WaitForShardServersLocalInit();
        _communicator.Value.BroadcastToShardServers(new CampaignInitializedPacket());
        _initRepo.Value.IsInitialized = true;
        
        const int initialPauseDuration = 15;
        _logger.Value.LogInformation("Initialized, unpausing in {duration} seconds", initialPauseDuration);
        await _timeProvider.Value.Delay(TimeSpan.FromSeconds(initialPauseDuration));
        _synchronizedCampaignStateRepo.Value.Unpause();
        
        const int sessionDuration = 60;
        _logger.Value.LogInformation("Unpaused, the session ends in {duration} seconds", sessionDuration);
        await _timeProvider.Value.Delay(TimeSpan.FromSeconds(sessionDuration));
        _synchronizedCampaignStateRepo.Value.Pause();
        
        await TrySaveSnapshotAsync();
        _logger.Value.LogInformation("Session ended");
    }
    
    private void CreateSingletonServices()
    {
        _communicator.Value = ServiceProvider.GetRequiredService<IFromCampaignServerCommunicator>();
        _snapshotManager.Value = ServiceProvider.GetRequiredService<CampaignSnapshotManager>();
        _snapshotSerializer.Value = ServiceProvider.GetRequiredService<ICampaignSnapshotSerializer>();
        _userRepo.Value = ServiceProvider.GetRequiredService<IUserRepository>();
        _synchronizedCampaignStateRepo.Value =
            ServiceProvider.GetRequiredService<ISynchronizedCampaignStateRepository>();
        _timeProvider.Value = ServiceProvider.GetRequiredService<TimeProvider>();
        _logger.Value = ServiceProvider.GetRequiredService<ILogger<CampaignServerMain>>();
        _initRepo.Value = ServiceProvider.GetRequiredService<IInitializationRepository>();
    }
    
    private async Task TryLoadSnapshotAsync()
    {
        if (!IsSingleplayer && File.Exists(SnapshotPath))
        {
            _logger.Value.LogInformation("Loading a snapshot");
            byte[] bytes = File.ReadAllBytes(SnapshotPath);
            CampaignSnapshot snapshot = _snapshotSerializer.Value.Deserialize(bytes);
            await _snapshotManager.Value.ReplicateSnapshotAsync(snapshot);
        }
        else
        {
            _logger.Value.LogInformation("Starting a new campaign");
            _communicator.Value.BroadcastToShardServers(new NoInitialShardSnapshotPacket());
        }
    }
    
    private async Task WaitForShardServersLocalInit()
    {
        _logger.Value.LogInformation("Waiting for shard servers to finish local initializing");
        Task timeout = Task.Delay(TimeSpan.FromSeconds(30));
        await Task.WhenAny(timeout, Task.WhenAll(_shardServerLocalInitDoneTcs.Values.Select(it => it.Task)));
        ImmutableList<Guid> uninitializedShardServerIds = _shardServerLocalInitDoneTcs
            .Where(it => !it.Value.Task.IsCompleted)
            .Select(it => it.Key)
            .ToImmutableList();
        if (uninitializedShardServerIds.Count > 1)
        {
            string idsStr = uninitializedShardServerIds.JoinToString(", ");
            throw new TimeoutException($"Shard servers {idsStr} didn't finish local initializing in time");
        }
    }
    
    private async Task TrySaveSnapshotAsync()
    {
        if (!IsSingleplayer)
        {
            CampaignSnapshot snapshot = await _snapshotManager.Value.CreateSnapshotAsync();
            byte[] bytes = _snapshotSerializer.Value.Serialize(snapshot);
            File.WriteAllBytes(SnapshotPath, bytes);
        }
    }
    
    private string SnapshotPath
    {
        get
        {
            return Path.Combine
            (
                ServiceProvider.GetRequiredService<CampaignPersistenceOptions>().SnapshotFolder,
                "Snapshot"
            );
        }
    }

    public override void _Process(float delta)
    {
        _communicator.Value.Poll();
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<ICampaignServerInitPacketReceiver>(this);
        services.AddSingleton<IInitializationRepository, InitializationRepository>();
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
        
        Config.AddToServiceCollection(services);
        Logging.AddToServiceCollection(services);
    }
    
    private void CreateSingletonNodes()
    {
        _processPublisher = new ProcessPublisher().Also(it => AddChild(it));
    }

    public void ReceiveShardServerInitAwaitingCampaignServerInitPacket(Guid senderId)
    {
        _shardServerLocalInitDoneTcs[senderId].SetResult();
    }
}
