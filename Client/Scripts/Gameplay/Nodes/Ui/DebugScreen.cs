using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Ui;

public sealed class DebugScreen : Label
{
    private const float UpdateInterval = 0.2f;
    
    private float _timeSinceUpdate;
    
    private readonly IPingMeasurer _pingMeasurer;
    private readonly IShardServiceProviderSource _shardServiceProviderSource;
    
    public DebugScreen(IPingMeasurer pingMeasurer, IShardServiceProviderSource shardServiceProviderSource)
    {
        Visible = false;
        
        _pingMeasurer = pingMeasurer;
        _shardServiceProviderSource = shardServiceProviderSource;
    }
    
    public override void _Process(float delta)
    {
        if (IsServer)
        {
            QueueFree();
            return;
        }
        
        _timeSinceUpdate += delta;
        if (_timeSinceUpdate >= UpdateInterval)
        {
            UpdateText(delta);
            _timeSinceUpdate = 0;
        }
    }
    
    private void UpdateText(float delta)
    {
        float fps = 1 / delta;
        float? ping = _pingMeasurer.Ping(Const.TestShardId);
        ISynchronizationClient? synchronizationClient = _shardServiceProviderSource.ShardServiceProviders
            .GetOrDefault(Const.TestShardId)
            ?.GetRequiredService<ISynchronizationClient>();
        
        Text =
            $"""
             fps: {fps:N0}
             ping: {ToMillisecondsString(ping)}
             sync latency: {ToMillisecondsString(synchronizationClient?.Latency)}
             """;
    }
    
    private string ToMillisecondsString(float? seconds) =>
        seconds == null ? "?" : (seconds.Value * 1000).ToString("N0") + "ms";

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("debug_screen")) Visible = !Visible;
    }
}