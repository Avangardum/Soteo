using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.PacketHandlers;
using Soteo.Gameplay.Resources;
using Soteo.Gameplay.Services;
using Soteo.Gameplay.Services.Communicators;
using Soteo.Gameplay.Services.Synchronization;
using Soteo.Gameplay.Ui;
using Soteo.Gameplay.Util;
using Soteo.Shared;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.PacketSerializers;

namespace Soteo.Gameplay;

public sealed class Main : Node2D, IShardLoader, IShardServiceProviderSource
{
    // This class handles scene loading and dependency injection.
    // A service scope corresponds to a shard.
    // Server simulates a single shard, so it creates a scope on startup and uses it for everything.
    // Client can connect to multiple shards, so it uses a separate scope for each loaded shard.
    
    private Hud? _hud;
    private Node2D? _shardRoot;
    private WebSocketFromGameplayToCampaignServerCommunicator? _webSocketCampaignServerCommunicator;
    private WebRtcFromGameplayToGameplayCommunicator? _webRtcGameplayCommunicator;
    private JsmqFromGameplayCommunicator? _jsmqCommunicator;
    
    private PackedScene? _shardScene;
    private IServiceProvider? _rootServiceProvider;
    private Shard? _newScopeShard;
    private readonly Dictionary<Guid, IServiceScope> _shardServiceScopes = [];
    
    // Fields for running a shard server from the editor
    public static bool EditorIsServer { get; private set; }
    public static Guid EditorLocalShardServerId { get; private set; }
    [Export] private bool _editorIsServer = false;
    [Export] private string _editorLocalShardServerId = "";

    public IReadOnlyDictionary<Guid, IServiceProvider> ShardServiceProviders =>
        _shardServiceScopes.ToImmutableDictionary(it => it.Key, it => it.Value.ServiceProvider);

    public override void _EnterTree()
    {
        EditorIsServer = _editorIsServer;
        EditorLocalShardServerId = Guid.Parse(_editorLocalShardServerId);
    }

    public override void _Ready()
    {
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        _rootServiceProvider = serviceCollection.BuildAutofacServiceProvider();
        GetNodes();
        CreateNodes();
        
        _shardScene = ResourceLoader.Load<PackedScene>("res://Scenes/Shard.tscn");
        
        if (IsServer) LoadShard();
    }
    
    private void GetNodes()
    {
        _shardRoot = GetNode<Node2D>("Shards");
    }
    
    private void CreateNodes()
    {
        if (UseJsmq)
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
        
        if (IsServer)
        {
            
        }
        else
        {
            var ui = GetNode<CanvasLayer>("Ui");
            _hud = ActivatorUtilities.CreateInstance<Hud>(_rootServiceProvider);
            ui.AddChild(_hud);
            AddChild(ActivatorUtilities.CreateInstance<InputHandler>(_rootServiceProvider));
            ui.AddChild(ActivatorUtilities.CreateInstance<LogInUi>(_rootServiceProvider));
            ui.AddChild(ActivatorUtilities.CreateInstance<DebugScreen>(_rootServiceProvider));
        }
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        RegisterSharedServices(services);
        if (IsServer)
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
        services.AddSingleton<IShardServiceProviderSource>(this);
        services.AddSingleton<ICurrentUserIdRepository, CurrentUserIdRepository>();
        services.AddSingleton<IPacketHandler, RoutingPacketHandler>();
        services.AddSingleton<IPacketSerializer, RoutingPacketSerializer>();
        services.AddSingleton<IEntityNodePool>(new EntityNodePool());
        services.AddSingleton<IChunkCollector, ChunkCollector>();
        
        services.AddScoped<IShard>(
            _ => _newScopeShard ?? throw new InvalidOperationException("This scope doesn't have a shard"));
        services.AddShardScopedNode<IEntityManager, EntityManager>();
        
        foreach (Type type in TypeLocator.PacketHandlerTypes.Values) services.AddTransient(type);
        
        if (UseJsmq)
        {
            services.AddSingleton<ICampaignServerCommunicator>(_ => _jsmqCommunicator.Required);
            services.AddSingleton<IPacketSender>(_ => _jsmqCommunicator.Required);
            services.AddSingleton<INetworkDebugger>(_ => _jsmqCommunicator.Required);
        }
        else
        {
            services.AddSingleton<ICampaignServerCommunicator>(_ => _webSocketCampaignServerCommunicator.Required);
            services.AddSingleton<IPacketSender>(_ => new RoutingPacketSender(
                _webSocketCampaignServerCommunicator.Required, _webRtcGameplayCommunicator.Required));
            services.AddSingleton<IWebrtcPacketReceiver>(_ => _webRtcGameplayCommunicator.Required);
            services.AddSingleton<INetworkDebugger>(_ => _webRtcGameplayCommunicator.Required);
        }
    }
    
    private void RegisterServerServices(IServiceCollection services)
    {
        
    }
    
    private void RegisterClientServices(IServiceCollection services)
    {
        services.AddShardScopedNode<ISynchronizationClient, SynchronizationClient>();
        services.AddSingletonNode<ICamera>("Camera");
        services.AddSingleton<IHud>(_ => _hud.Required);
        services.AddSingleton<IEntityLocator, EntityLocator>();
        services.AddSingleton<IPalette>(ResourceLoader.Load<Palette>("res://Palette.tres"));
    }
    
    public void LoadShard()
    {
        string mapPath = "res://Scenes/Maps/Test.tscn";
        Guid shardId = Const.TestShardId;
        Vector2 position = new Vector2(0, 0);

        var shard = _shardScene.Required.Instance<Shard>();
        shard.Id = shardId;
        shard.Name = shardId.ToString();
        shard.Position = position;
        _shardRoot.Required.AddChild(shard);
        
        var map = ResourceLoader.Load<PackedScene>(mapPath).Instance<Node2D>();
        shard.GetNode<Node2D>("Map").AddChild(map);
        
        var scope = _rootServiceProvider.Required.CreateScope();
        _newScopeShard = shard;
        scope.ServiceProvider.GetRequiredService<IShard>();
        _newScopeShard = null;
        CreateShardNodes(shard, scope.ServiceProvider);
        _shardServiceScopes[shardId] = scope;
    }
    
    private void CreateShardNodes(Shard shard, IServiceProvider serviceProvider)
    {
        shard.AddChild(ActivatorUtilities.CreateInstance<EntityManager>(serviceProvider));
        
        if (IsServer)
        {
            shard.AddChild(ActivatorUtilities.CreateInstance<SynchronizationServer>(serviceProvider));
        }
        else
        {
            shard.AddChild(ActivatorUtilities.CreateInstance<SynchronizationClient>(serviceProvider));
            shard.GetNode("Ui").AddChild(ActivatorUtilities.CreateInstance<OverheadUiManager>(serviceProvider));
        }
    }
}