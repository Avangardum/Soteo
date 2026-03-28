using Godot;

namespace Soteo.Client
{
    public class NetworkManager : Node
    {
        [Export] private string _serverIp = "localhost";
        [Export] private int _serverPort = 23137;
        
        private bool _isStarted;
        private PackedScene _playerScene;
        private SceneTree _sceneTree;
        private TickCounter _tickCounter;
        
        public override void _Ready()
        {
            _playerScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
            _sceneTree = GetTree();
            _sceneTree.Connect("network_peer_connected", this, nameof(OnNetworkPeerConnected));
            _sceneTree.Connect("network_peer_disconnected", this, nameof(OnNetworkPeerDisconnected));
            _sceneTree.Connect("connected_to_server", this, nameof(OnConnectedToServer));
            _sceneTree.Connect("connection_failed", this, nameof(OnConnectionFailed));
            _sceneTree.Connect("server_disconnected", this, nameof(OnServerDisconnected));
            _tickCounter = GetNode<TickCounter>("/root/Main/TickCounter");
        }
        
        public override void _Process(float delta)
        {
            if (!_isStarted) StartIfRequested();
        }
        
        private void StartIfRequested()
        {
            if (Input.IsActionJustPressed("ui_home"))
            {
                _isStarted = true;
                var peer = new WebSocketServer();
                peer.Listen(_serverPort, gdMpApi: true);
                GetTree().NetworkPeer = peer;
                _tickCounter.IsInitialized = true;
                GD.Print($"Server hosted at port {_serverPort}.");
            }
            else if (Input.IsActionJustPressed("ui_end"))
            {
                _isStarted = true;
                var peer = new WebSocketClient();
                peer.ConnectToUrl($"{_serverIp}:{_serverPort}", gdMpApi: true);
                GetTree().NetworkPeer = peer;
                GD.Print($"Connecting to {_serverIp}:{_serverPort}...");
            }
        }
        
        public void OnNetworkPeerConnected(int id)
        {
            GD.Print($"Peer {id} connected.");
            int playerId = id == 1 ? _sceneTree.GetNetworkUniqueId() : id;
            var player = _playerScene.Instance<Player>();
            player.Id = playerId;
            player.IsLocal = playerId == _sceneTree.GetNetworkUniqueId();
            player.Name = playerId.ToString();
            player.GetNode<Label>("Label").Text = playerId.ToString();
            GetNode("/root/Main/Players").AddChild(player);
        }
        
        public void OnNetworkPeerDisconnected(int id)
        {
            GD.Print($"Peer {id} disconnected.");
            if (id != 1) GetNode($"/root/Main/Players/{id}").QueueFree();
        }
        
        public void OnConnectedToServer()
        {
            GD.Print($"Connected to server.");
            GetNode<PingIndicator>("/root/Main/PingIndicator").IsConnected = true;
        }
        
        public void OnConnectionFailed()
        {
            GD.Print($"ConnectionFailed.");
        }
        
        public void OnServerDisconnected()
        {
            GD.Print($"Disconnected from server.");
            GetNode<PingIndicator>("/root/Main/PingIndicator").IsConnected = true;
        }
    }
}
