using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Nodes;

public sealed class Shard : Node2D, IShard
{
    public Guid Id { get; set; }
    
    public Node2D PlayerCharacterRoot { get; private set; } = null!;
    public Node2D NonPlayerCharacterRoot { get; private set; } = null!;
    public Node2D BuildingRoot { get; private set; } = null!;
    public Node2D ProjectileRoot { get; private set; } = null!;

    public override void _Ready()
    {
        PlayerCharacterRoot = GetNode<Node2D>("Entities/PlayerCharacters");
        NonPlayerCharacterRoot = GetNode<Node2D>("Entities/NonPlayerCharacters");
        BuildingRoot = GetNode<Node2D>("Entities/Buildings");
        ProjectileRoot = GetNode<Node2D>("Entities/Projectiles");
    }
}