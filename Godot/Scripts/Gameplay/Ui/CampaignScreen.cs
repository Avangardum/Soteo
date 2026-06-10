using System.Collections.Immutable;
using Soteo.Core;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;
using Soteo.Main.Gameplay.Interfaces;

namespace Soteo.Main.Gameplay.Ui;

public sealed class CampaignScreen
{
    private readonly IShardServerConnector _shardServerConnector;
    private readonly IFromGameplayPacketSender _packetSender;
    private readonly IShardLoader _shardLoader;
    private readonly ICurrentCharacterIdRepository _currentCharIdRepo;
    private readonly IVisibleShardIdRepository _visibleShardIdRepo;
    
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
        IFromGameplayPacketSender packetSender,
        IShardLoader shardLoader,
        ICurrentCharacterIdRepository currentCharIdRepo,
        IVisibleShardIdRepository visibleShardIdRepo
    )
    {
        _shardServerConnector = shardServerConnector;
        _packetSender = packetSender;
        _shardLoader = shardLoader;
        _currentCharIdRepo = currentCharIdRepo;
        _visibleShardIdRepo = visibleShardIdRepo; 
        
        node.Visible = false;
        _node = node;
        _characterButtons = node.GetNode("CharacterButtons").GetChildren().Cast<Button>().ToImmutableList();
        _shardButtons = node.GetNode("ShardButtons").GetChildren().Cast<Button>().ToImmutableList();
        
        campaignServerConnector.Connected += () => _node.Visible = true;
        node.GetNode<Button>("DeployButton").Connect("pressed", Deploy);
        node.GetNode<Button>("SpectateButton").Connect("pressed", Spectate);
        
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
        _currentCharIdRepo.Value = _selectedCharacterId;
        _visibleShardIdRepo.Value = _selectedShardId;
        _node.Visible = false;
    }
    
    private void Spectate()
    {
        if (_selectedShardId == null) return;
        
        _shardServerConnector.ConnectToShardServer(_selectedShardId.Value);
        _shardLoader.LoadShard(_selectedShardId.Value);
        _visibleShardIdRepo.Value = _selectedShardId;
        _node.Visible = false;
    }
}
