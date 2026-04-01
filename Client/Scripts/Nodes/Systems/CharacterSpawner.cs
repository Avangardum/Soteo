using Soteo.Client.Interfaces;

namespace Soteo.Client.Nodes.Systems;

public sealed class CharacterSpawner : Node, ICharacterSpawner
{
    private PackedScene _playerCharacterScene = null!;
    
    public override void _Ready()
    {
        _playerCharacterScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
    }

    public void SpawnPlayerCharacter(Guid characterId)
    {
        var playerCharacter = _playerCharacterScene.Instance<PlayerCharacter>();
        playerCharacter.Name = characterId.ToString();
        AddChild(playerCharacter); // todo use some other node as parent
    }
}