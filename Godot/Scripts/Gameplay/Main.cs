using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Gameplay;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.PacketHandlers;
using Soteo.Core.Gameplay.Services;
using Soteo.Core.Gameplay.Services.Synchronization;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.PacketSerializers;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes;
using Soteo.Gameplay.Resources;
using Soteo.Gameplay.Services;
using Soteo.Gameplay.Services.Communicators;
using Soteo.Gameplay.Ui;
using Soteo.Shared;
using Soteo.Shared.Nodes;

namespace Soteo.Gameplay;

public sealed class Main : Node2D, IShardLoader
{
    // This class handles scene loading and dependency injection.
    // A service scope corresponds to a shard.
    // Server simulates a single shard, so it creates a scope on startup and uses it for everything.
    // Client can connect to multiple shards, so it uses a separate scope for each loaded shard.
    
    private Hud? _hud;
    private DebugScreenNode? _debugScreenNode;
    private Node2D? _shardRoot;
    private WebSocketFromGameplayToCampaignServerCommunicator? _webSocketCampaignServerCommunicator;
    private WebRtcFromGameplayToGameplayCommunicator? _webRtcGameplayCommunicator;
    private JsmqFromGameplayCommunicator? _jsmqCommunicator;
    private ProcessPublisher? _processPublisher;
    
    private PackedScene? _shardScene;
    private IServiceProvider? _rootServiceProvider;
    private ShardNode? _newScopeShard;
    private readonly Dictionary<Guid, IServiceScope> _shardServiceScopes = [];
    
    // Fields for running a shard server from the editor
    public static bool EditorIsServer { get; private set; }
    public static Guid EditorLocalShardServerId { get; private set; }
    [Export] private bool _editorIsServer;
    [Export] private string _editorLocalShardServerId = "";

    public override void _EnterTree()
    {
        EditorIsServer = _editorIsServer;
        EditorLocalShardServerId = Guid.Parse(_editorLocalShardServerId);
    }

    public override void _Ready()
    {
        ConstInitializer.Init();
        TypeLocator.Init(CoreGameplayAssembly.Value, CoreSharedAssembly.Value);
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        _rootServiceProvider = serviceCollection.BuildAutofacServiceProvider();
        GetNodes();
        CreateNodes();
        CreateSingletonServices(_rootServiceProvider);
        
        _shardScene = ResourceLoader.Load<PackedScene>("res://Scenes/Shard.tscn");
        
        if (Const.IsServer) LoadShard();
    }
    
    private void GetNodes()
    {
        _shardRoot = GetNode<Node2D>("Shards");
    }
    
    private void CreateNodes()
    {
        _processPublisher = new ProcessPublisher();
        AddChild(_processPublisher);
        
        if (Const.UseJsmq)
        {
            _jsmqCommunicator = ActivatorUtilities.CreateInstance<JsmqFromGameplayCommunicator>(_rootServiceProvider.Required);
            AddChild(_jsmqCommunicator);
        }
        else
        {
            _webSocketCampaignServerCommunicator =
                ActivatorUtilities.CreateInstance<WebSocketFromGameplayToCampaignServerCommunicator>(_rootServiceProvider.Required);
            AddChild(_webSocketCampaignServerCommunicator);
            _webRtcGameplayCommunicator =
                ActivatorUtilities.CreateInstance<WebRtcFromGameplayToGameplayCommunicator>(_rootServiceProvider);
            AddChild(_webRtcGameplayCommunicator);
        }
        
        if (Const.IsServer)
        {
            
        }
        else
        {
            var ui = GetNode<CanvasLayer>("Ui");
            _hud = ActivatorUtilities.CreateInstance<Hud>(_rootServiceProvider);
            ui.AddChild(_hud);
            AddChild(ActivatorUtilities.CreateInstance<InputHandler>(_rootServiceProvider));
            ui.AddChild(ActivatorUtilities.CreateInstance<LogInUi>(_rootServiceProvider));
            _debugScreenNode = DebugScreenNode.Instance();
            ui.AddChild(_debugScreenNode);
        }
    }
    
    private void CreateSingletonServices(IServiceProvider serviceProvider)
    {
        if (!Const.IsServer)
            serviceProvider.GetRequiredService<DebugScreen>();
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        RegisterSharedServices(services);
        if (Const.IsServer)
            RegisterServerServices(services);
        else
            RegisterClientServices(services);
    }
    
    private void RegisterSharedServices(IServiceCollection services)
    {
        services.AddTransient(typeof(ServerDependency<>));
        services.AddTransient(typeof(ClientDependency<>));
        
        services.AddSingleton(this);
        services.AddSingleton<IShardLoader>(this);
        services.AddSingleton<IShardServiceProviders>(new ShardServiceProviders(_shardServiceScopes));
        services.AddSingleton<ICurrentUserIdRepository, CurrentUserIdRepository>();
        services.AddSingleton<IPacketHandler, GameplayRoutingPacketHandler>();
        services.AddSingleton<IPacketSerializer, RoutingPacketSerializer>();
        services.AddSingleton<IEntityNodePool>(new EntityNodePool());
        services.AddSingleton<IChunkCollector, ChunkCollector>();
        services.AddSingleton<IProcessPublisher>(_ => _processPublisher.Required);
        services.AddSingleton<IFrameStopwatch, FrameStopwatch>();
        
        services.AddScoped<ShardNode>(
            _ => _newScopeShard ?? throw new InvalidOperationException("This scope doesn't have a shard"));
        services.AddAlias<IShard, ShardNode>();
        services.AddScoped<EntityManager>();
        services.AddAlias<IEntityManager, EntityManager>();
        services.AddAlias<IEntitySnapshotManager, EntityManager>();
        services.AddScoped<IEntityNodeManager, EntityNodeManager>();
        
        foreach (Type type in PacketHandler.TypesByPacketType.Values)
            services.AddTransient(type);
        
        if (Const.UseJsmq)
        {
            services.AddSingleton<ICampaignServerCommunicator>(_ => _jsmqCommunicator.Required);
            services.AddSingleton<IPacketSender>(_ => _jsmqCommunicator.Required);
            services.AddSingleton<INetworkDebugger>(_ => _jsmqCommunicator.Required);
            services.AddSingleton<IConnectionNotifier>(_ => _jsmqCommunicator.Required);
        }
        else
        {
            services.AddSingleton<ICampaignServerCommunicator>(_ => _webSocketCampaignServerCommunicator.Required);
            services.AddSingleton<IPacketSender>(_ => new RoutingPacketSender(
                _webSocketCampaignServerCommunicator.Required, _webRtcGameplayCommunicator.Required));
            services.AddSingleton<IWebrtcPacketReceiver>(_ => _webRtcGameplayCommunicator.Required);
            services.AddSingleton<INetworkDebugger>(_ => _webRtcGameplayCommunicator.Required);
            services.AddSingleton<IConnectionNotifier>(_ => _webRtcGameplayCommunicator.Required);
        }
    }
    
    private void RegisterServerServices(IServiceCollection services)
    {
        services.AddScoped<ISynchronizationServer, SynchronizationServer>();
    }
    
    private void RegisterClientServices(IServiceCollection services)
    {
        services.AddScoped<ISynchronizationClient, SynchronizationClient>();
        services.AddSingletonNode<ICamera>("Camera");
        services.AddSingleton<IHud>(_ => _hud.Required);
        services.AddSingleton<DebugScreenNode>(_ => _debugScreenNode.Required);
        services.AddSingleton<DebugScreen>();
        services.AddSingleton<IEntityLocator, EntityLocator>();
        services.AddSingleton<IPalette>(ResourceLoader.Load<Palette>("res://Palette.tres"));
        services.AddSingletonNode<ITooltip>("Ui/TooltipLayer/Tooltip");
        services.AddSingleton<ILocalizer, Localizer>();
    }
    
    public void LoadShard()
    {
        string mapPath = "res://Scenes/Maps/Test.tscn";
        Guid shardId = Const.TestShardId;
        Vector2 position = new Vector2(0, 0);

        var shard = _shardScene.Required.Instance<ShardNode>();
        shard.Id = shardId;
        shard.Name = shardId.ToString();
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
        _shardServiceScopes[shardId] = scope;
    }
    
    private void CreateShardScopedNodes(ShardNode shard, IServiceProvider serviceProvider)
    {
        if (Const.IsServer) return;
        shard.GetNode("Ui").AddChild(ActivatorUtilities.CreateInstance<OverheadUiManager>(serviceProvider));
    }
    
    private void CreateShardScopedServices(IServiceProvider serviceProvider)
    {
        if (Const.IsServer)
            serviceProvider.GetRequiredService<ISynchronizationServer>();
    }
}
