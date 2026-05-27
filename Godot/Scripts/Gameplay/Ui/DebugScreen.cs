using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Extensions;
using Soteo.Util;

namespace Soteo.Gameplay.Ui;

public sealed class DebugScreen : Control
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/DebugScreen.tscn"); 
 
    private const int RingLength = 10 * Const.TicksPerSecond;
    private readonly double[] _fpsRing = new double[RingLength];
    private readonly double[] _unrolledFpsRing = new double[RingLength];
    private readonly double[] _entityCountRing = new double[RingLength];
    private readonly double[] _unrolledEntityCountRing = new double[RingLength];
    private int _ringNextIndex;
    private int _pendingProcessCount;
    
    private readonly INetworkDebugger _networkDebugger;
    private readonly IShardServiceProviderSource _shardServiceProviderSource;
    
    private readonly Label _label;
    private readonly Graph _fpsGraph;
    private readonly Graph _serverLoadGraph;
    private readonly Graph _entityCountGraph;
    
    public DebugScreen(INetworkDebugger networkDebugger, IShardServiceProviderSource shardServiceProviderSource)
    {
        _networkDebugger = networkDebugger;
        _shardServiceProviderSource = shardServiceProviderSource;
        
        Name = nameof(DebugScreen);
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
        AnchorRight = 1;
        AnchorBottom = 1;
        
        Scene.InstanceAndReparentTo(this);
        _label = GetNode<Label>("Label");
        _fpsGraph = GetNode<Graph>("FpsGraph");
        _serverLoadGraph = GetNode<Graph>("ServerLoadGraph");
        _entityCountGraph = GetNode<Graph>("EntityCountGraph");
    }
    
    public override void _PhysicsProcess(float delta)
    {
        _pendingProcessCount++;
    }

    public override void _Process(float delta)
    {
        while (_pendingProcessCount > 0)
        {
            _pendingProcessCount--;
            IServiceProvider? shardServiceProvider = _shardServiceProviderSource.ShardServiceProviders
                .GetOrDefault(Const.TestShardId);
            var synchronizationClient = shardServiceProvider?.GetRequiredService<ISynchronizationClient>();
            var entityManager = shardServiceProvider?.GetRequiredService<IEntityManager>();

            _ringNextIndex = (_ringNextIndex + 1) % RingLength;
            ProcessFpsGraph(delta);
            ProcessEntityCountGraph(entityManager);
            if (synchronizationClient != null)
                _serverLoadGraph.SetData(synchronizationClient.ServerLoadHistory, "N2", 0, 1);

            UpdateText(delta, synchronizationClient, entityManager);
        }
    }

    private void ProcessFpsGraph(double delta)
    {
        _fpsRing[_ringNextIndex] = 1 / delta;
        _fpsRing.UnrollRingTo(_unrolledFpsRing, _ringNextIndex + 1);
        _fpsGraph.SetData(_unrolledFpsRing, "N0", 0);
    }
    
    private void ProcessEntityCountGraph(IEntityManager? entityManager)
    {
        _entityCountRing[_ringNextIndex] = entityManager?.Entities.Count ?? 0;
        _entityCountRing.UnrollRingTo(_unrolledEntityCountRing, _ringNextIndex + 1);
        _entityCountGraph.SetData(_unrolledEntityCountRing, "N0", 0);
    }
    
    private void UpdateText(double delta, ISynchronizationClient? synchronizationClient, IEntityManager? entityManager)
    {
        _label.Text =
            $"""
             fps: {1 / delta :N0}
             ping: {ToMillisecondsString(_networkDebugger.Ping(Const.TestShardId))}
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

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("debug_screen"))
            Visible = !Visible;
    }
}
