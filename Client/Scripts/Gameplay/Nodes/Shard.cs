using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Nodes;

public sealed class Shard : Node2D, IShard
{
    public Guid Id { get; set; }
    public Node2D Node => this;
    public Node2D EntityRoot { get; private set; } = null!;

    public override void _Ready()
    {
        EntityRoot = GetNode<Node2D>("Entities");
    }
}