using Soteo.Client.Interfaces;

namespace Soteo.Client.Nodes.Systems;

public sealed class EntitySpawner : Node, IEntitySpawner
{
    private IEntityRoots _entityRoots = null!;
    private PackedScene _playerCharacterScene = null!;
    
    public void Inject(IEntityRoots shard)
    {
        _entityRoots = shard;
    }
    
    public override void _Ready()
    {
        _playerCharacterScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
    }

    public void SpawnPlayerCharacter(Guid characterId)
    {
        var playerCharacter = _playerCharacterScene.Instance<PlayerCharacter>();
        playerCharacter.Name = characterId.ToString();
        _entityRoots.PlayerCharacterRoot.AddChild(playerCharacter);
    }
}