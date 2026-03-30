using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Client;

public sealed class ServiceProvider : Node, IServiceProvider
{
    private IServiceProvider _innerProvider = null!;
    
    public static ServiceProvider Instance { get; private set; } = null!;
    
    private MasterServerCommunicator _masterServerCommunicator = null!;
    private CharacterSpawner _characterSpawner = null!;
    private ClientShardServerCommunicator _clientShardServerCommunicator = null!;
    
    public override void _Ready()
    {
        if (Instance != null) throw new InvalidOperationException();
        Instance = this;
        
        GetNodes();
        var serviceCollection = new ServiceCollection();
        RegisterServices(serviceCollection);
        _innerProvider = serviceCollection.BuildServiceProvider();
        InjectIntoAutoloads();
    }
    
    private void GetNodes()
    {
        _masterServerCommunicator = GetNode<MasterServerCommunicator>("/root/MasterServerCommunicator");
        _clientShardServerCommunicator = GetNode<ClientShardServerCommunicator>("/root/ClientShardServerCommunicator");
        _characterSpawner = GetNode<CharacterSpawner>("/root/CharacterSpawner");
    }
    
    public void RegisterServices(IServiceCollection services)
    {
        foreach (Type type in TypeLocator.PacketHandlerTypes.Values) services.AddTransient(type);

        services.AddSingleton<IMasterServerCommunicator>(_masterServerCommunicator);
        services.AddSingleton<IPacketSender>(
            new UniversalPacketSender(_masterServerCommunicator, _clientShardServerCommunicator));
        services.AddSingleton<ICharacterSpawner>(_characterSpawner);
        services.AddSingleton<IWebRtcSignalingReceiver>(_clientShardServerCommunicator);
    }
    
    private void InjectIntoAutoloads()
    {
        _clientShardServerCommunicator.Inject(_masterServerCommunicator, this);
    }

    public object? GetService(Type serviceType) => _innerProvider.GetService(serviceType);
}