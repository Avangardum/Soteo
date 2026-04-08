using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Systems;
using Soteo.Gameplay.Nodes.Systems.Communicators;
using Soteo.Gameplay.PacketHandlers;
using Soteo.Shared;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes;

public sealed class Main : Node2D, IShardLoader, IShardServiceProvider
{
    // This class handles scene loading and dependency injection.
    // A service scope corresponds to a shard.
    // Server simulates a single shard, so it creates a scope on startup and uses it for everything.
    // Client can connect to multiple shards, so it uses a separate scope for each loaded shard.
    
    private LogIn _logIn = null!;
    private Node2D _shardRoot = null!;
    private MasterServerCommunicator _masterServerCommunicator = null!;
    private ClientShardServerCommunicator _clientShardServerCommunicator = null!;
    
    private PackedScene _shardScene = null!;
    private IServiceProvider _rootServiceProvider = null!;
    private Shard? _newScopeShard;
    private readonly Dictionary<Guid, IServiceScope> _shardServiceScopes = [];
    
    // Fields for running a shard server from the editor
    public static bool EditorIsServer { get; private set; }
    public static Guid EditorLocalShardServerId { get; private set; }
    [Export] private bool _editorIsServer = false;
    [Export] private string _editorLocalShardServerId = "";

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
            _logIn.Visible = false;
            LoadShard();
        }
        
        if (EditorIsServer) GetNode<Button>("UI/ConnectAsServerButton").Visible = true;
    }
    
    private void GetNodes()
    {
        _logIn = GetNode<LogIn>("UI/LogIn");
        _shardRoot = GetNode<Node2D>("Shards");
        _masterServerCommunicator = GetNode<MasterServerCommunicator>("Systems/MasterServerCommunicator");
        _clientShardServerCommunicator =
            GetNode<ClientShardServerCommunicator>("Systems/ClientShardServerCommunicator");
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IShardLoader>(this);
        services.AddSingleton<IShardServiceProvider>(this);
        services.AddSingleton<IMasterServerCommunicator>(_masterServerCommunicator);
        services.AddSingleton<IPacketSender>(
            new RoutingPacketSender(_masterServerCommunicator, _clientShardServerCommunicator));
        services.AddSingleton<IWebRtcSignalingReceiver>(_clientShardServerCommunicator);
        services.AddSingleton<IUserIdRepository, UserIdRepository>();
        services.AddSingleton<IPacketHandler, RoutingPacketHandler>();
        
        services.AddScoped<Shard>(
            _ => _newScopeShard ?? throw new InvalidOperationException("This scope doesn't have a shard"));
        services.AddScoped<IEntityRoots>(sp => sp.GetService<Shard>()!);
        services.AddScoped<IEntitySpawner>(
            sp => sp.GetRequiredService<Shard>().GetNode<EntitySpawner>("Systems/EntitySpawner"));
        
        foreach (Type type in TypeLocator.PacketHandlerTypes.Values) services.AddTransient(type);
    }
    
    private void InjectInto(Node node, IServiceProvider serviceProvider)
    {
        List<MethodInfo> injectMethods = node.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(it => it.Name == "Inject")
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

    public IServiceProvider? GetServiceProviderForShard(Guid id) => _shardServiceScopes.GetOrDefault(id)?.ServiceProvider;
}