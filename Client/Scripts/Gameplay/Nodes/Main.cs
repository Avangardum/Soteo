using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Systems;
using Soteo.Gameplay.Nodes.Systems.Communicators;
using Soteo.Gameplay.Nodes.Systems.Synchronization;
using Soteo.Gameplay.Nodes.Ui;
using Soteo.Gameplay.PacketHandlers;
using Soteo.Gameplay.Resources;
using Soteo.Shared;
using Soteo.Shared.Attributes;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.PacketSerializers;

namespace Soteo.Gameplay.Nodes;

public sealed class Main : Node2D, ISceneLoader, IShardServiceProviderSource
{
    // This class handles scene loading and dependency injection.
    // A service scope corresponds to a shard.
    // Server simulates a single shard, so it creates a scope on startup and uses it for everything.
    // Client can connect to multiple shards, so it uses a separate scope for each loaded shard.
    
    private LogInUi _logInUi = null!;
    private Node2D _shardRoot = null!;
    private WebSocketMasterServerCommunicator _webSocketMasterServerCommunicator = null!;
    private WebRtcGameplayCommunicator _webRtcGameplayCommunicator = null!;
    private JsmqCommunicator _jsmqCommunicator = null!;
    
    private Dictionary<string, PackedScene> _scenes = [];
    private PackedScene _shardScene = null!;
    private IServiceProvider _rootServiceProvider = null!;
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
        GetNodes();
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        _rootServiceProvider = new SimpleServiceProvider(serviceCollection);
        InjectInto(this, _rootServiceProvider);
        
        _shardScene = ResourceLoader.Load<PackedScene>("res://Scenes/Shard.tscn");
        
        if (IsServer)
        {
            _logInUi.Visible = false;
            LoadShard();
        }
        
        if (EditorIsServer) GetNode<Button>("Ui/ConnectAsServerButton").Visible = true;
    }
    
    private void GetNodes()
    {
        _logInUi = GetNode<LogInUi>("Ui/LogIn");
        _shardRoot = GetNode<Node2D>("Shards");
        _webSocketMasterServerCommunicator =
            GetNode<WebSocketMasterServerCommunicator>("Systems/WebSocketMasterServerCommunicator");
        _webRtcGameplayCommunicator = GetNode<WebRtcGameplayCommunicator>("Systems/WebRtcGameplayCommunicator");
        _jsmqCommunicator = GetNode<JsmqCommunicator>("Systems/JsmqCommunicator");
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        RegisterSharedServices(services);
        if (IsServer) RegisterServerServices(services);
        else RegisterClientServices(services);
    }
    
    private void RegisterSharedServices(IServiceCollection services)
    {
        services.AddTransient(typeof(ServerDependency<>));
        services.AddTransient(typeof(ClientDependency<>));
        
        services.AddSingleton(this);
        services.AddSingleton<ISceneLoader>(this);
        services.AddSingleton<IShardServiceProviderSource>(this);
        services.AddSingleton<ICurrentUserIdRepository, CurrentUserIdRepository>();
        services.AddSingleton<IPacketHandler, RoutingPacketHandler>();
        services.AddSingleton<IPacketSerializer, RoutingPacketSerializer>();
        
        services.AddScoped<Shard>(
            _ => _newScopeShard ?? throw new InvalidOperationException("This scope doesn't have a shard"));
        services.AddAlias<IShard, Shard>(); // todo reference IShard only
        services.AddShardScopedNode<IEntityManager, EntityManager>();
        
        foreach (Type type in TypeLocator.PacketHandlerTypes.Values) services.AddTransient(type);
        
        if (UseJsmq)
        {
            services.AddSingleton<IMasterServerCommunicator>(_jsmqCommunicator);
            services.AddSingleton<IPacketSender>(_jsmqCommunicator);
        }
        else
        {
            services.AddSingleton<IMasterServerCommunicator>(_webSocketMasterServerCommunicator);
            services.AddSingleton<IPacketSender>(
                new RoutingPacketSender(_webSocketMasterServerCommunicator, _webRtcGameplayCommunicator));
            services.AddSingleton<IWebrtcPacketReceiver>(_webRtcGameplayCommunicator);
        }
    }
    
    private void RegisterServerServices(IServiceCollection services)
    {
        
    }
    
    private void RegisterClientServices(IServiceCollection services)
    {
        services.AddShardScopedNode<ISynchronizationPacketReceiver, SynchronizationClient>();
        services.AddSingletonNode<ICamera>("Camera");
        services.AddSingletonNode<IHud>("Ui/Hud");
        services.AddSingleton<IEntityLocator, EntityLocator>();
        services.AddSingleton<IPalette>(ResourceLoader.Load<Palette>("res://Palette.tres"));
    }
    
    private void InjectInto(Node node, IServiceProvider serviceProvider)
    {
        if (!IsInstanceValid(node) || node.IsQueuedForDeletion()) return;
        
        List<MethodInfo> injectMethods = node.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(it => it.HasAttribute<InjectAttribute>())
            .ToList();
        switch (injectMethods.Count)
        {
            case > 1:
                throw new InvalidOperationException($"{node.GetType()} has multiple Inject methods");
            case 1:
                MethodInfo injectMethod = injectMethods.Single();
                object[] parameters = injectMethod.GetParameters()
                    .Select(it => serviceProvider.GetRequiredService(it.ParameterType))
                    .ToArray();
                injectMethod.Invoke(node, parameters);
                break;
        }
        foreach (Node child in node.GetChildren()) InjectInto(child, serviceProvider);
    }
    
    public void LoadShard()
    {
        string mapPath = "res://Scenes/Maps/Test.tscn";
        Guid shardId = Const.TestShardId;
        Vector2 position = new Vector2(0, 0);

        var shard = _shardScene.Instance<Shard>();
        shard.Id = shardId;
        shard.Name = shardId.ToString();
        shard.Position = position;
        _shardRoot.AddChild(shard);
        
        var map = ResourceLoader.Load<PackedScene>(mapPath).Instance<Node2D>();
        shard.GetNode<Node2D>("Map").AddChild(map);
        
        var scope = _rootServiceProvider.CreateScope();
        _newScopeShard = shard;
        scope.ServiceProvider.GetRequiredService<Shard>();
        _newScopeShard = null;
        InjectInto(shard, scope.ServiceProvider);
        _shardServiceScopes[shardId] = scope;
    }
    
    public T InstanceScene<T>(string path, Guid shardId, Func<Type, IServiceProvider, Node> nodeFactory) where T : Node
    {
        PackedScene scene = _scenes.GetOrAdd(path, () => ResourceLoader.Load<PackedScene>(path));
        Node node = scene.Instance();
        node = ReplaceProxies(node, shardId, nodeFactory);
        return (T)node;
    }
    
    private Node ReplaceProxies(Node node, Guid shardId, Func<Type, IServiceProvider, Node> nodeFactory) // todo Guid?
    {
        if (node is Proxy proxy)
        {
            IServiceProvider serviceProvider =
                shardId == Guid.Empty ? _rootServiceProvider : ShardServiceProviders[shardId];
            Node replacement = nodeFactory(proxy.ReplacementType, serviceProvider);
            if (replacement.GetType() != proxy.ReplacementType) throw new InvalidOperationException(
                $"nodeFactory returned {replacement.GetType()}, but expected {proxy.ReplacementType}");
            proxy.ReplaceBy(replacement);
            proxy.QueueFree();
            node = replacement;
        }
        
        foreach (Node child in node.GetChildren()) ReplaceProxies(child, shardId, nodeFactory);
        
        return node;
    }
}