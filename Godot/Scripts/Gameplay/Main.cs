using Microsoft.Extensions.DependencyInjection;
using Soteo.Core;
using Soteo.Core.Attributes;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.Services;
using Soteo.Core.Services.PacketHandlers.Gameplay;
using Soteo.Core.Services.Repositories;
using Soteo.Core.Services.Serializers;
using Soteo.Core.Services.Serializers.PacketSerializers;
using Soteo.Core.Services.Synchronization;
using Soteo.Core.SidedDependencies;
using Soteo.Core.StaticHelpers;
using Soteo.Main.Gameplay.Interfaces;
using Soteo.Main.Gameplay.Resources;
using Soteo.Main.Gameplay.Services;
using Soteo.Main.Gameplay.Services.Communicators;
using Soteo.Main.Gameplay.Ui;
using Soteo.Main.Shared;
using Soteo.Main.Shared.Nodes;

namespace Soteo.Main.Gameplay;

public sealed class Main : Node2D, IShardLoader
{
    // This class handles scene loading and dependency injection.
    // A service scope corresponds to a shard.
    // Server simulates a single shard, so it creates a scope on startup and uses it for everything.
    // Client can connect to multiple shards, so it uses a separate scope for each loaded shard.
    
    private readonly bool _useJsmq = OS.HasFeature("web") && SharedCmdLineArgs.IsSingleplayer;

    private LogInScreenNode? _logIScreenNode;
    private HudNode? _hudNode;
    private DebugScreenNode? _debugScreenNode;
    private CampaignScreenNode? _campaignScreenNode;
    private Node2D? _shardRoot;
    private WebSocketFromGameplayToCampaignServerCommunicator? _webSocketCampaignServerCommunicator;
    private WebRtcFromGameplayToGameplayCommunicator? _webRtcGameplayCommunicator;
    private JsmqFromGameplayCommunicator? _jsmqCommunicator;
    private ProcessPublisher? _processPublisher;
    
    private PackedScene? _shardScene;
    private IServiceProvider? _rootServiceProvider;
    private ShardNode? _newScopeShard;
    private readonly Dictionary<Guid, IServiceScope> _shardServiceScopes = [];
    
    public override void _Ready()
    {
        GlobalInit.Init();
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        _rootServiceProvider = serviceCollection.BuildAutofacServiceProvider();
        GetNodes();
        CreateSingletonNodes();
        CreateSingletonServices(_rootServiceProvider);
        
        _shardScene = ResourceLoader.Load<PackedScene>("res://Scenes/Shard.tscn");
        
        if (SharedCmdLineArgs.Side == Side.ShardServer)
        {
            LoadShard(_rootServiceProvider.GetRequiredService<ICurrentUserIdRepository>().Required);
            if (!SharedCmdLineArgs.IsSingleplayer)
                _rootServiceProvider.GetRequiredService<IPauseRepository>().IsPaused = true;
        }
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        RegisterSharedServices(services);
        
        if (SharedCmdLineArgs.Side == Side.ShardServer)
            RegisterServerServices(services);
        else
            RegisterClientServices(services);
        
        if (_useJsmq)
            RegisterJsmqServices(services);
        else
            RegisterWebServices(services);
    }
    
    private void RegisterSharedServices(IServiceCollection services)
    {
        services.AddSingleton(this);
        services.AddSingleton<IShardLoader>(this);
        services.AddSingleton<IShardServiceProviders>(new ShardServiceProviders(_shardServiceScopes));
        services.AddSingleton<ICurrentUserIdRepository, CurrentUserIdRepository>();
        services.AddSingleton<IPacketHandler, GameplayRoutingPacketHandler>();
        services.AddSingleton<IPacketSerializer, RoutingPacketSerializer>();
        services.AddSingleton<IEntityNodePool, EntityNodePool>();
        services.AddSingleton<IProcessPublisher>(_ => _processPublisher.Required);
        services.AddSingleton<IFrameStopwatch, FrameStopwatch>();
        services.AddSingletonNode<IPauseRepository>("/root/PauseRepository");
        services.AddSingleton<ISideDetector>(new SideDetector(SharedCmdLineArgs.Side));
        services.AddSingleton<ISerializationHelper, SerializationHelper>();
        var typeLocator = new TypeLocator(SoteoCoreAssembly.Value);
        services.AddSingleton<ITypeLocator>(typeLocator);
        services.AddScoped<ShardNode>
        (
            _ => _newScopeShard ?? throw new InvalidOperationException("This scope doesn't have a shard")
        );
        services.AddAlias<IShard, ShardNode>();
        services.AddScoped<EntityManager>();
        services.AddAlias<IEntityManager, EntityManager>();
        services.AddAlias<IEntitySnapshotManager, EntityManager>();
        services.AddScoped<IEntityNodeManager, EntityNodeManager>();
        services.AddSingleton<TimeProvider>(new GodotTimeProvider(GetTree()));
        
        foreach (Type type in PacketSerializer.AllTypes(typeLocator))
            services.AddSingleton(type);
        
        foreach (Type type in PacketHandlerLocator<GameplayPacketHandlerAttribute>.AllTypes(typeLocator))
            services.AddScoped(type);
    }
    
    private void RegisterServerServices(IServiceCollection services)
    {
        services.AddTransient(typeof(ServerDependency<>), typeof(ServerDependency<>.NotNull));
        services.AddTransient(typeof(ClientDependency<>), typeof(ClientDependency<>.Null));
        
        services.AddScoped<ISynchronizationServer, SynchronizationServer>();
        services.AddScoped<ICurrentTickRepository, CurrentTickRepository>();
        services.AddScoped<IShardPersistenceSnapshotManager, ShardPersistenceSnapshotManager>();
    }
    
    private void RegisterClientServices(IServiceCollection services)
    {
        services.AddTransient(typeof(ServerDependency<>), typeof(ServerDependency<>.Null));
        services.AddTransient(typeof(ClientDependency<>), typeof(ClientDependency<>.NotNull));
        
        services.AddSingleton<LogInScreenNode>(_ => _logIScreenNode.Required);
        services.AddSingleton<LogInScreen>();
        services.AddSingletonNode<ICamera>("Camera");
        services.AddSingleton<HudNode>(_ => _hudNode.Required);
        services.AddSingleton<IHud, Hud>();
        services.AddSingleton<DebugScreenNode>(_ => _debugScreenNode.Required);
        services.AddSingleton<DebugScreen>();
        services.AddSingleton<CampaignScreenNode>(_ => _campaignScreenNode.Required);
        services.AddSingleton<CampaignScreen>();
        services.AddSingleton<IEntityLocator, EntityLocator>();
        services.AddSingleton<IPalette>(ResourceLoader.Load<Palette>("res://Palette.tres"));
        services.AddSingletonNode<ITooltip>("Ui/TooltipLayer/Tooltip");
        services.AddSingleton<ILocalizer, Localizer>();
        services.AddSingleton<ICurrentCharacterIdRepository, CurrentCharacterIdRepository>();
        services.AddSingleton<IVisibleShardIdRepository, VisibleShardIdRepository>();
        
        services.AddScoped<ISynchronizationClient, SynchronizationClient>();
    }
    
    private void RegisterJsmqServices(IServiceCollection services)
    {
        services.AddSingleton<ICampaignServerConnector>(_ => _jsmqCommunicator.Required);
        services.AddSingleton<IFromGameplayToCampaignServerPacketSender>(_ => _jsmqCommunicator.Required);
        services.AddSingleton<IFromGameplayPacketSender>(_ => _jsmqCommunicator.Required);
        services.AddSingleton<INetworkDebugger>(_ => _jsmqCommunicator.Required);
        services.AddSingleton<IConnectionNotifier>(_ => _jsmqCommunicator.Required);
        services.AddSingleton<IShardServerConnector>(_ => _jsmqCommunicator.Required);
    }
    
    private void RegisterWebServices(IServiceCollection services)
    {
        services.AddSingleton<ICampaignServerConnector>(_ => _webSocketCampaignServerCommunicator.Required);
        services.AddSingleton<IFromGameplayToCampaignServerPacketSender>(_ => _webSocketCampaignServerCommunicator.Required);
        services.AddSingleton<IFromGameplayPacketSender>
        (
            _ => new RoutingPacketSender
            (
                _webSocketCampaignServerCommunicator.Required,
                _webRtcGameplayCommunicator.Required
            )
        );
        services.AddSingleton<IWebrtcPacketReceiver>(_ => _webRtcGameplayCommunicator.Required);
        services.AddSingleton<INetworkDebugger>(_ => _webRtcGameplayCommunicator.Required);
        services.AddSingleton<IConnectionNotifier>(_ => _webRtcGameplayCommunicator.Required);
        services.AddSingleton<IShardServerConnector>(_ => _webRtcGameplayCommunicator.Required);
        services.AddSingleton<IChunkCollector, ChunkCollector>();
    }
    
    private void GetNodes()
    {
        _shardRoot = GetNode<Node2D>("Shards");
    }
    
    private void CreateSingletonNodes()
    {
        _processPublisher = new ProcessPublisher().Also(it => AddChild(it));
        
        if (_useJsmq)
        {
            _jsmqCommunicator = ActivatorUtilities
                .CreateInstance<JsmqFromGameplayCommunicator>(_rootServiceProvider.Required)
                .Also(it => AddChild(it));
        }
        else
        {
            _webSocketCampaignServerCommunicator = ActivatorUtilities
                .CreateInstance<WebSocketFromGameplayToCampaignServerCommunicator>(_rootServiceProvider.Required)
                .Also(it => AddChild(it));
            _webRtcGameplayCommunicator = ActivatorUtilities
                .CreateInstance<WebRtcFromGameplayToGameplayCommunicator>(_rootServiceProvider)
                .Also(it => AddChild(it));
        }
        
        if (SharedCmdLineArgs.Side == Side.Client)
        {
            var ui = GetNode<CanvasLayer>("Ui").Required;
            _hudNode = HudNode.Instance().Also(it => ui.AddChild(it));
            AddChild(ActivatorUtilities.CreateInstance<InputHandler>(_rootServiceProvider));
            _logIScreenNode = LogInScreenNode.Instance().Also(it => ui.AddChild(it));
            _debugScreenNode = DebugScreenNode.Instance().Also(it => ui.AddChild(it));
            _campaignScreenNode = CampaignScreenNode.Instance().Also(it => ui.AddChild(it));
        }
    }
    
    private void CreateSingletonServices(IServiceProvider serviceProvider)
    {
        if (SharedCmdLineArgs.Side == Side.Client)
        {
            serviceProvider.GetRequiredService<LogInScreen>();
            serviceProvider.GetRequiredService<DebugScreen>();
            serviceProvider.GetRequiredService<IHud>();
            serviceProvider.GetRequiredService<CampaignScreen>();
        }
    }
    
    private void CreateShardScopedNodes(ShardNode shard, IServiceProvider serviceProvider)
    {
        if (SharedCmdLineArgs.Side == Side.ShardServer) return;
        shard.GetNode("Ui").AddChild(ActivatorUtilities.CreateInstance<OverheadUiManager>(serviceProvider));
    }
    
    private void CreateShardScopedServices(IServiceProvider serviceProvider)
    {
        if (SharedCmdLineArgs.Side == Side.ShardServer)
            serviceProvider.GetRequiredService<ISynchronizationServer>();
    }
    
    public void LoadShard(Guid id)
    {
        string mapPath = $"res://Scenes/Maps/Test{id.ToString()[^1]}.tscn";
        Vector2 position = new Vector2(0, 0);

        var shard = _shardScene.Required.Instance<ShardNode>();
        shard.Id = id;
        shard.Name = id.ToString();
        shard.Position = position.ToGd();
        _shardRoot.Required.AddChild(shard);
        
        var map = ResourceLoader.Load<PackedScene>(mapPath).Instance<Node2D>();
        shard.GetNode<Node2D>("Map").AddChild(map);
        
        var scope = _rootServiceProvider.Required.CreateScope();
        _newScopeShard = shard;
        scope.ServiceProvider.GetRequiredService<ShardNode>();
        _newScopeShard = null;
        CreateShardScopedNodes(shard, scope.ServiceProvider);
        CreateShardScopedServices(scope.ServiceProvider);
        _shardServiceScopes[id] = scope;
    }
}
