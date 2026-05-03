using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Ui;

public sealed class DebugScreen : Control
{
    private const double UpdateInterval = 0.2;
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/DebugScreen.tscn"); 
    
    private double _timeSinceUpdate;
    private readonly double[] _fpsRing = new double[1000];
    private readonly double[] _unrolledFpsRing = new double[1000];
    private int _fpsRingNextIndex;
    private double _fpsSamplingCountdown;
    
    private readonly INetworkDebugger _networkDebugger;
    private readonly IShardServiceProviderSource _shardServiceProviderSource;
    
    private readonly Label _label;
    private readonly Graph _fpsGraph;
    
    public DebugScreen(INetworkDebugger networkDebugger, IShardServiceProviderSource shardServiceProviderSource)
    {
        _networkDebugger = networkDebugger;
        _shardServiceProviderSource = shardServiceProviderSource;
        
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
        AnchorRight = 1;
        AnchorBottom = 1;
        
        Scene.InstanceAndReparentTo(this);
        _label = GetNode<Label>("Label");
        _fpsGraph = GetNode<Graph>("FpsGraph");
    }
    
    public override void _Process(float delta)
    {
        ProcessFpsGraph(delta);
        
        _timeSinceUpdate += delta;
        if (_timeSinceUpdate >= UpdateInterval)
        {
            UpdateText();
            _timeSinceUpdate = 0;
        }
    }
    
    private void ProcessFpsGraph(double delta)
    {
        const double fpsSamplingInterval = 0.02;
        _fpsSamplingCountdown -= delta;
        while (_fpsSamplingCountdown <= 0)
        {
            _fpsSamplingCountdown += fpsSamplingInterval;
            _fpsRing[_fpsRingNextIndex] = Engine.GetFramesPerSecond();
            _fpsRingNextIndex = (_fpsRingNextIndex + 1) % _fpsRing.Length;
        }
        _fpsRing.UnrollRingTo(_unrolledFpsRing, _fpsRingNextIndex);
        _fpsGraph.SetData(_unrolledFpsRing, "N0", 0);
    }
    
    private void UpdateText()
    {
        ISynchronizationClient? synchronizationClient = _shardServiceProviderSource.ShardServiceProviders
            .GetOrDefault(Const.TestShardId)
            ?.GetRequiredService<ISynchronizationClient>();
        
        _label.Text =
            $"""
             fps: {Engine.GetFramesPerSecond():N0}
             ping: {ToMillisecondsString(_networkDebugger.Ping(Const.TestShardId))}
             sync latency: {ToMillisecondsString(synchronizationClient?.Latency)}
             wait frames: {synchronizationClient?.WaitFrameCount ?? 0}
             fast-forwards: {synchronizationClient?.FastForwardCount ?? 0}
             bytes sent: {_networkDebugger.BytesSent:N0}
             bytes received: {_networkDebugger.BytesReceived:N0}
             """;
    }
    
    private string ToMillisecondsString(double? seconds) =>
        seconds == null ? "?" : (seconds.Value * 1000).ToString("N0") + "ms";

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("debug_screen")) Visible = !Visible;
    }
}