using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Soteo.Core;
using Soteo.Core.Campaign;
using Soteo.Core.Networking;
using Soteo.Core.PacketHandlers;
using Soteo.Core.PacketHandlers.CampaignServer;
using Soteo.Core.Packets;
using Soteo.Core.Persistence;
using Soteo.Core.Serialization;
using Soteo.Core.Serialization.Packets;
using Soteo.Core.Snapshots;
using File = System.IO.File;
using Path = System.IO.Path;

namespace Soteo.Main.CampaignServer;

public sealed partial class CampaignServerMain : Node, ICampaignServerInitPacketReceiver
{
    private readonly Dictionary<Guid, TaskCompletionSource> _shardServerLocalInitDoneTcs = new();

    private IFromCampaignServerCommunicator _communicator = null!;
    private IServiceProvider _serviceProvider = null!;
    private CampaignSnapshotManager _snapshotManager = null!;
    private ICampaignSnapshotSerializer _snapshotSerializer = null!;
    private IUserRepository _userRepo = null!;
    private ISynchronizedCampaignStateRepository _synchronizedCampaignStateRepo = null!;
    private TimeProvider _timeProvider = null!;
    private ILogger<CampaignServerMain> _logger = null!;
    private IInitializationRepository _initRepo = null!;
    private IProcessPublisher _processPublisher = null!;

    public override void _Ready()
    {
        InitAsync().CollectException();
    }

    private async Task InitAsync()
    {
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        CreateSingletonNodes();
        _serviceProvider = serviceCollection.BuildServiceProvider();
        CreateSingletonServices();

        IReadOnlyList<Guid> shardIds = _serviceProvider.GetRequiredService<CampaignOptions>().ShardIds;

        _logger.LogInformation("Waiting for shard servers to connect");
        await _userRepo.WaitForUsersToConnectAsync(shardIds, timeout: 30);

        // Create a task for each shard server waiting for it to finish local initializing, which is everything
        // except for waiting for other servers' initialization. Once all the shard servers sent that, we can
        // tell them to complete initialization.
        foreach (Guid id in shardIds)
            _shardServerLocalInitDoneTcs[id] = new TaskCompletionSource();

        await TryLoadSnapshotAsync();

        await WaitForShardServersLocalInit();
        _communicator.BroadcastToShardServers(new CampaignInitializedPacket());
        _initRepo.IsInitialized = true;

        const int initialPauseDuration = 10;
        _logger.LogInformation("Initialized, unpausing in {duration} seconds", initialPauseDuration);
        await _timeProvider.Delay(TimeSpan.FromSeconds(initialPauseDuration));
        _synchronizedCampaignStateRepo.Unpause();

        const int sessionDuration = 120;
        _logger.LogInformation("Unpaused, the session ends in {duration} seconds", sessionDuration);
        await _timeProvider.Delay(TimeSpan.FromSeconds(sessionDuration));
        _synchronizedCampaignStateRepo.Pause();

        await TrySaveSnapshotAsync();
        _logger.LogInformation("Session ended");
    }

    private void CreateSingletonServices()
    {
        _communicator = _serviceProvider.GetRequiredService<IFromCampaignServerCommunicator>();
        _snapshotManager = _serviceProvider.GetRequiredService<CampaignSnapshotManager>();
        _snapshotSerializer = _serviceProvider.GetRequiredService<ICampaignSnapshotSerializer>();
        _userRepo = _serviceProvider.GetRequiredService<IUserRepository>();
        _synchronizedCampaignStateRepo =
            _serviceProvider.GetRequiredService<ISynchronizedCampaignStateRepository>();
        _timeProvider = _serviceProvider.GetRequiredService<TimeProvider>();
        _logger = _serviceProvider.GetRequiredService<ILogger<CampaignServerMain>>();
        _initRepo = _serviceProvider.GetRequiredService<IInitializationRepository>();
    }

    private async Task TryLoadSnapshotAsync()
    {
        if (File.Exists(SnapshotPath))
        {
            _logger.LogInformation("Loading a snapshot");
            byte[] bytes = File.ReadAllBytes(SnapshotPath);
            CampaignSnapshot snapshot = _snapshotSerializer.Deserialize(bytes);
            await _snapshotManager.ReplicateSnapshotAsync(snapshot);
        }
        else
        {
            _logger.LogInformation("Starting a new campaign");
            _communicator.BroadcastToShardServers(new NoInitialShardSnapshotPacket());
        }
    }

    private async Task WaitForShardServersLocalInit()
    {
        _logger.LogInformation("Waiting for shard servers to finish local initializing");
        Task timeout = Task.Delay(TimeSpan.FromSeconds(30));
        await Task.WhenAny(timeout, Task.WhenAll(_shardServerLocalInitDoneTcs.Values.Select(it => it.Task)));
        IReadOnlyList<Guid> uninitializedShardServerIds = _shardServerLocalInitDoneTcs
            .Where(it => !it.Value.Task.IsCompleted)
            .Select(it => it.Key)
            .ToList().AsReadOnly();
        if (uninitializedShardServerIds.Count > 0)
        {
            string idsStr = uninitializedShardServerIds.JoinToString(", ");
            throw new TimeoutException($"Shard servers {idsStr} didn't finish local initializing in time");
        }
    }

    private async Task TrySaveSnapshotAsync()
    {
        CampaignSnapshot snapshot = await _snapshotManager.CreateSnapshotAsync();
        byte[] bytes = _snapshotSerializer.Serialize(snapshot);
        File.WriteAllBytes(SnapshotPath, bytes);
    }

    private string SnapshotPath
    {
        get
        {
            return Path.Combine
            (
                _serviceProvider.GetRequiredService<CampaignPersistenceOptions>().SnapshotFolder,
                "Snapshot"
            );
        }
    }

    public override void _Process(double delta)
    {
        _communicator.Poll();
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
        services.AddSingleton<SerializationHelper, SerializationHelper>();
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
        services.AddSingleton<IShardServerAllowlist>(sp =>
            ShardServerAllowlist.Enabled(sp.GetRequiredService<CampaignOptions>().ShardIds));
        services.AddSingleton<ISynchronizedCampaignStateRepository, SynchronizedCampaignStateRepository>();
        services.AddSingleton<IProcessPublisher>(_ => _processPublisher);
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
