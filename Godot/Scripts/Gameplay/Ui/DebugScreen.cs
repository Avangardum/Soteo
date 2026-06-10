using Microsoft.Extensions.DependencyInjection;
using Soteo.Core;
using Soteo.Core.Interfaces;

namespace Soteo.Main.Gameplay.Ui;

public sealed class DebugScreen
{
    private const int RingLength = 10 * Const.TicksPerSecond;
    private readonly double[] _fpsRing = new double[RingLength];
    private readonly double[] _entityCountRing = new double[RingLength];
    private readonly double[] _serverLoadRing = new double[RingLength];
    private readonly double[] _unrolledRing = new double[RingLength];
    private int _ringNextIndex;
    private int _pendingProcessCount;
    
    private readonly INetworkDebugger _networkDebugger;
    private readonly IShardServiceProviders _shardServiceProviders;
    private readonly IVisibleShardIdRepository _visibleShardIdRepository;

    private readonly DebugScreenNode _node;
    private readonly Label _label;
    private readonly Graph _fpsGraph;
    private readonly Graph _serverLoadGraph;
    private readonly Graph _entityCountGraph;
    
    public DebugScreen
    (
        DebugScreenNode node,
        INetworkDebugger networkDebugger,
        IShardServiceProviders shardServiceProviders,
        IVisibleShardIdRepository visibleShardIdRepository
    )
    {
        _networkDebugger = networkDebugger;
        _shardServiceProviders = shardServiceProviders;
        _visibleShardIdRepository = visibleShardIdRepository;

        node.DebugScreen = this;
        node.Visible = false;
        node.PauseMode = Node.PauseModeEnum.Process;
        _node = node;
        _label = node.GetNode<Label>("Label");
        _fpsGraph = node.GetNode<Graph>("FpsGraph");
        _serverLoadGraph = node.GetNode<Graph>("ServerLoadGraph");
        _entityCountGraph = node.GetNode<Graph>("EntityCountGraph");
    }
    
    public void PhysicsProcess(double delta)
    {
        _pendingProcessCount++;
    }
    
    public void Process(double delta)
    {
        while (_pendingProcessCount > 0)
        {
            _pendingProcessCount--;
            IServiceProvider? shardServiceProvider =
                _visibleShardIdRepository.Value?.PassTo(_shardServiceProviders.GetOrDefault);
            var synchronizationClient = shardServiceProvider?.GetRequiredService<ISynchronizationClient>();
            var entityManager = shardServiceProvider?.GetRequiredService<IEntityManager>();

            _ringNextIndex = (_ringNextIndex + 1) % RingLength;
            ProcessFpsGraph(delta);
            ProcessEntityCountGraph(entityManager);
            ProcessServerLoadGraph(synchronizationClient);

            UpdateText(delta, synchronizationClient, entityManager);
        }
    }

    private void ProcessFpsGraph(double delta)
    {
        _fpsRing[_ringNextIndex] = 1 / delta;
        _fpsRing.UnrollRingTo(_unrolledRing, _ringNextIndex + 1);
        _fpsGraph.SetData(_unrolledRing, "N0", 0);
    }
    
    private void ProcessEntityCountGraph(IEntityManager? entityManager)
    {
        _entityCountRing[_ringNextIndex] = entityManager?.Entities.Count ?? 0;
        _entityCountRing.UnrollRingTo(_unrolledRing, _ringNextIndex + 1);
        _entityCountGraph.SetData(_unrolledRing, "N0", 0);
    }
    
    private void ProcessServerLoadGraph(ISynchronizationClient? syncClient)
    {
        _serverLoadRing[_ringNextIndex] = syncClient?.ServerLoad ?? 0;
        _serverLoadRing.UnrollRingTo(_unrolledRing, _ringNextIndex + 1);
        _serverLoadGraph.SetData(_unrolledRing, "N2", min: 0, max: 1);
    }
    
    private void UpdateText(double delta, ISynchronizationClient? synchronizationClient, IEntityManager? entityManager)
    {
        _label.Text =
            $"""
             fps: {1 / delta :N0}
             ping: {ToMillisecondsString(_visibleShardIdRepository.Value?.PassTo(_networkDebugger.Ping))}
             sync latency: {ToMillisecondsString(synchronizationClient?.Latency)}
             wait frames: {synchronizationClient?.WaitFrameCount ?? 0 :N0}
             fast-forwards: {synchronizationClient?.FastForwardCount ?? 0 :N0}
             bytes sent: {_networkDebugger.BytesSent:N0}
             bytes received: {_networkDebugger.BytesReceived:N0}
             entities: {entityManager?.Entities.Count ?? 0 :N0}
             """;
    }
    
    private string ToMillisecondsString(double? seconds) =>
        seconds == null ? "?" : (seconds.Value * 1000).ToString("N0") + "ms";

    public void UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("debug_screen"))
            _node.Visible = !_node.Visible;
    }
}
