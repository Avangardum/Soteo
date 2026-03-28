using System;
using Godot;

namespace Soteo.Client;

public class PingIndicator : Label
{
    private DateTime? _pingTime;
    private SceneTree _sceneTree;
    
    public bool IsConnected { get; set; }

    public override void _Ready()
    {
        _sceneTree = GetTree();
    }

    public override void _PhysicsProcess(float delta)
    {
        if (!IsConnected) return;
        if (_pingTime != null) return;
        
        _pingTime = DateTime.Now;
        RpcId(1, nameof(Ping));
    }
    
    [Master]
    public void Ping()
    {
        RpcId(_sceneTree.GetRpcSenderId(), nameof(Pong));
    }
    
    [Puppet]
    public void Pong()
    {
        if (_pingTime == null) return;
        TimeSpan ping = DateTime.Now - _pingTime.Value;
        Text = $"Ping: {(int)ping.TotalMilliseconds}ms";
        _pingTime = null;
    }
}