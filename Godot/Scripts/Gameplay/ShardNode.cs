using Soteo.Core;

namespace Soteo.Main.Gameplay;

public sealed partial class ShardNode : Node2D, IShard
{
    public override void _Ready()
    {
        EntityRoot = GetNode<Node2D>("Entities");
    }

    public Node2D EntityRoot { get; private set; } = null!;
    public Guid Id { get; set; }
}
