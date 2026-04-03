using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Client.Interfaces;
using Soteo.Client.Nodes.Systems;
using Soteo.Shared;

namespace Soteo.Client.Nodes;

public sealed class Main : Node2D, IShardLoader
{
    private LogIn _logIn = null!;
    private Node2D _shardRoot = null!;
    private MasterServerCommunicator _masterServerCommunicator = null!;
    private CharacterSpawner _characterSpawner = null!;
    private ClientShardServerCommunicator _clientShardServerCommunicator = null!;
    
    private PackedScene _shardScene = null!;
    private IServiceProvider _serviceProvider = null!;
    
    public override void _Ready()
    {
        GetNodes();
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        _serviceProvider = new SimpleServiceProvider(serviceCollection);
        InjectInto(this);
        
        _shardScene = ResourceLoader.Load<PackedScene>("res://Scenes/Shard.tscn");
        
        if (IsServer)
        {
            _logIn.Visible = false;
            LoadShard();
        }
    }
    
    private void GetNodes()
    {
        _logIn = GetNode<LogIn>("UI/LogIn");
        _shardRoot = GetNode<Node2D>("Shards");
        _masterServerCommunicator = GetNode<MasterServerCommunicator>("Systems/MasterServerCommunicator");
        _clientShardServerCommunicator =
            GetNode<ClientShardServerCommunicator>("Systems/ClientShardServerCommunicator");
        _characterSpawner = GetNode<CharacterSpawner>("Systems/CharacterSpawner");
    }
    
    private void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IServiceProvider>(sp => sp);
        services.AddSingleton<IShardLoader>(this);
        services.AddSingleton<IMasterServerCommunicator>(_masterServerCommunicator);
        services.AddSingleton<IPacketSender>(
            new UniversalPacketSender(_masterServerCommunicator, _clientShardServerCommunicator));
        services.AddSingleton<ICharacterSpawner>(_characterSpawner);
        services.AddSingleton<IWebRtcSignalingReceiver>(_clientShardServerCommunicator);
        
        foreach (Type type in TypeLocator.PacketHandlerTypes.Values) services.AddTransient(type);
    }
    
    private void InjectInto(Node node)
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
                    .Select(it => _serviceProvider.GetRequiredService(it.ParameterType))
                    .ToArray();
                injectMethod.Invoke(node, parameters);
                break;
        }
        foreach (Node child in node.GetChildren()) InjectInto(child);
    }
    
    public void LoadShard()
    {
        string mapPath = "res://Scenes/Maps/Test.tscn";
        Guid shardId = Const.TestShardId;
        Vector2 position = new Vector2(0, 0);

        var shard = _shardScene.Instance<Node2D>();
        shard.Name = shardId.ToString();
        shard.Position = position;
        _shardRoot.AddChild(shard);
        
        var map = ResourceLoader.Load<PackedScene>(mapPath).Instance<Node2D>();
        shard.GetNode<Node2D>("Map").AddChild(map);
        
        InjectInto(shard);
    }
}