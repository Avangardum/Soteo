using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Attributes;

namespace Soteo.Gameplay.Nodes.Ui;

public sealed class DebugScreen : Label
{
    private const float UpdateInterval = 1;
    
    private float _timeSinceUpdate;
    
    private IPingMeasurer _pingMeasurer = null!;
    
    [Inject]
    public void Inject(IPingMeasurer pingMeasurer)
    {
        _pingMeasurer = pingMeasurer;
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
        
        Text =
            $"""
             fps: {fps:N0}
             ping: {(ping == null ? "?" : (ping.Value * 1000).ToString("N0") + "ms")}
             """;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("debug_screen")) Visible = !Visible;
    }
}