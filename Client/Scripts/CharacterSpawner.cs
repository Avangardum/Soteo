namespace Soteo.Client;

public sealed class CharacterSpawner : Node, ICharacterSpawner
{
    public static CharacterSpawner Instance { get; private set; } = null!;

    private PackedScene _playerCharacterScene = null!;
    
    public override void _Ready()
    {
        if (Instance != null) throw new InvalidOperationException();
        Instance = this;
        
        _playerCharacterScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
    }

    public void SpawnPlayerCharacter(Guid characterId)
    {
        var playerCharacter = _playerCharacterScene.Instance<PlayerCharacter>();
        playerCharacter.Name = characterId.ToString();
        AddChild(playerCharacter); // todo use some other node as parent
    }
}