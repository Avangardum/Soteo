using System;
using Godot;

namespace Soteo.Client
{
    public class TickCounter : Node
    {
        private SceneTree _tree;
        private bool _isConnectedToServer;
        private int _ticksPerSecond;
        
        public bool IsInitialized { get; set; }
        public long Tick { get; set; }
        public float TickProgress { get; private set; }
        
        public override void _Ready()
        {
            _tree = GetTree();
            _tree.Connect("connected_to_server", this, nameof(OnConnectedToServer));
            _tree.Connect("server_disconnected", this, nameof(OnServerDisconnected));
            _tree.Connect("network_peer_connected", this, nameof(OnNetworkPeerConnected));
            _ticksPerSecond = Engine.IterationsPerSecond;
        }

        public override void _PhysicsProcess(float delta)
        {
            if (!_tree.IsNetworkServer()) return;
            Tick++;
            TickProgress = 0;
        }

        public override void _Process(float delta)
        {
            if (!IsInitialized) return;
            
            if (_tree.IsNetworkServer())
            {
                TickProgress = Math.Min(TickProgress + delta, 1);
            }
            else
            {
                TickProgress += delta * _ticksPerSecond;
                Tick += (int)TickProgress;
                TickProgress %= 1;
            }
        }
        
        public void OnConnectedToServer()
        {
            _isConnectedToServer = true;
        }
        
        public void OnServerDisconnected()
        {
            _isConnectedToServer = false;
        }
        
        public void OnNetworkPeerConnected(int id)
        {
            if (!_tree.IsNetworkServer()) return;
            RpcId(id, nameof(Initialize), Tick, TickProgress);
        }
        
        [Puppet]
        public void Initialize(long ticksElapsed, float tickProgress)
        {
            Tick = ticksElapsed - 2;
            TickProgress = tickProgress;
            IsInitialized = true;
        }
    }
}
