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

    private IFromCampaignServerCommunicator? _communicator;
    private IServiceProvider? _serviceProvider;
    private CampaignSnapshotManager? _snapshotManager;
    private ICampaignSnapshotSerializer? _snapshotSerializer;
    private IUserRepository? _userRepo;
    private ISynchronizedCampaignStateRepository? _synchronizedCampaignStateRepo;
    private ILogger<CampaignServerMain>? _logger;
    private IInitializationRepository? _initRepo;
    private IProcessPublisher? _processPublisher;

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

        _logger.Required().LogInformation("Waiting for shard servers to connect");
        await _userRepo.Required().WaitForUsersToConnectAsync(shardIds, timeout: 30);

        // Create a task for each shard server waiting for it to finish local initializing, which is everything
        // except for waiting for other servers' initialization. Once all the shard servers sent that, we can
        // tell them to complete initialization.
        foreach (Guid id in shardIds)
            _shardServerLocalInitDoneTcs[id] = new TaskCompletionSource();

        await TryLoadSnapshotAsync();

        await WaitForShardServersLocalInit();
        _communicator.Required().BroadcastToShardServers(new CampaignInitializedPacket());
        _initRepo.Required().IsInitialized = true;

        const int initialPauseDuration = 10;
        _logger.LogInformation("Initialized, unpausing in {duration} seconds", initialPauseDuration);
        await Task.Delay(TimeSpan.FromSeconds(initialPauseDuration));
        _synchronizedCampaignStateRepo.Required().Unpause();

        const int sessionDuration = 120;
        _logger.LogInformation("Unpaused, the session ends in {duration} seconds", sessionDuration);
        await Task.Delay(TimeSpan.FromSeconds(sessionDuration));
        _synchronizedCampaignStateRepo.Pause();

        await TrySaveSnapshotAsync();
        _logger.LogInformation("Session ended");
    }

    private void CreateSingletonServices()
    {
        _communicator = _serviceProvider.Required().GetRequiredService<IFromCampaignServerCommunicator>();
        _snapshotManager = _serviceProvider.GetRequiredService<CampaignSnapshotManager>();
        _snapshotSerializer = _serviceProvider.GetRequiredService<ICampaignSnapshotSerializer>();
        _userRepo = _serviceProvider.GetRequiredService<IUserRepository>();
        _synchronizedCampaignStateRepo =
            _serviceProvider.GetRequiredService<ISynchronizedCampaignStateRepository>();
        _logger = _serviceProvider.GetRequiredService<ILogger<CampaignServerMain>>();
        _initRepo = _serviceProvider.GetRequiredService<IInitializationRepository>();
    }

    private async Task TryLoadSnapshotAsync()
    {
        if (File.Exists(SnapshotPath))
        {
            _logger.Required().LogInformation("Loading a snapshot");
            byte[] bytes = File.ReadAllBytes(SnapshotPath);
            CampaignSnapshot snapshot = _snapshotSerializer.Required().Deserialize(bytes);
            await _snapshotManager.Required().ReplicateSnapshotAsync(snapshot);
        }
        else
        {
            _logger.Required().LogInformation("Starting a new campaign");
            _communicator.Required().BroadcastToShardServers(new NoInitialShardSnapshotPacket());
        }
    }

    private async Task WaitForShardServersLocalInit()
    {
        _logger.Required().LogInformation("Waiting for shard servers to finish local initializing");
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
        CampaignSnapshot snapshot = await _snapshotManager.Required().CreateSnapshotAsync();
        byte[] bytes = _snapshotSerializer.Required().Serialize(snapshot);
        File.WriteAllBytes(SnapshotPath, bytes);
    }

    private string SnapshotPath
    {
        get
        {
            return Path.Combine
            (
                _serviceProvider.Required().GetRequiredService<CampaignPersistenceOptions>().SnapshotFolder,
                "Snapshot"
            );
        }
    }

    public override void _Process(double delta)
    {
        _communicator.Required().Poll();
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
        services.AddSingleton<IProcessPublisher>(_ => _processPublisher.Required());
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
