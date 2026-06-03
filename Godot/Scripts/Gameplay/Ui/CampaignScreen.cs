using System.Collections.Immutable;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Packets;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Ui;

public sealed class CampaignScreen
{
    private readonly IShardServerConnector _shardServerConnector;
    private readonly IPacketSender _packetSender;
    private readonly IShardLoader _shardLoader;
    
    private readonly CampaignScreenNode _node;
    private readonly IReadOnlyList<Button> _characterButtons;
    private readonly IReadOnlyList<Button> _shardButtons;
    
    private Guid? _selectedCharacterId;
    private Guid? _selectedShardId;
    
    public CampaignScreen
    (
        CampaignScreenNode node,
        ICampaignServerConnector campaignServerConnector,
        IShardServerConnector shardServerConnector,
        IPacketSender packetSender,
        IShardLoader shardLoader
    )
    {
        _shardServerConnector = shardServerConnector;
        _packetSender = packetSender;
        _shardLoader = shardLoader;
        
        node.Visible = false;
        _node = node;
        _characterButtons = node.GetNode("CharacterButtons").GetChildren().Cast<Button>().ToImmutableList();
        _shardButtons = node.GetNode("ShardButtons").GetChildren().Cast<Button>().ToImmutableList();
        
        campaignServerConnector.Connected += () => _node.Visible = true;
        node.GetNode<Button>("DeployButton").Connect("pressed", Deploy);
        
        foreach (Button button in _characterButtons)
            button.Connect("pressed", () => SelectCharacter(Guid.Parse(button.Text)));
        
        foreach (Button button in _shardButtons)
            button.Connect("pressed", () => SelectShard(Guid.Parse(button.Text)));
    }
    
    private void SelectCharacter(Guid id)
    {
        foreach (Button button in _characterButtons)
            button.Pressed = button.Text == id.ToString();
        
        _selectedCharacterId = id;
    }
    
    private void SelectShard(Guid id)
    {
        foreach (Button button in _shardButtons)
            button.Pressed = button.Text == id.ToString();
        
        _selectedShardId = id;
    }
    
    private void Deploy()
    {
        if (_selectedCharacterId == null || _selectedShardId == null) return;
        
        _packetSender.SendReliable
        (
            new SpawnCharacterPacket
            {
                CharacterId = _selectedCharacterId.Value,
                PeerId = _selectedShardId.Value,
            },
            Const.CampaignServerId
        );
        
        _shardServerConnector.ConnectToShardServer(_selectedShardId.Value);
        _shardLoader.LoadShard(_selectedShardId.Value);
        _node.Visible = false;
    }
}
