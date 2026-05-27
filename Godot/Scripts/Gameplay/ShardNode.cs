using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay;

public sealed class ShardNode : Node2D, IShardNode
{
    public Guid Id { get; set; }
    public Node2D Node => this; // todo remove
    public Node2D EntityRoot { get; private set; } = null!; // todo lateinit

    public override void _Ready()
    {
        EntityRoot = GetNode<Node2D>("Entities");
    }
}
