using Soteo.Core.Interfaces;
using Soteo.Util;

namespace Soteo.Main.Gameplay;

public sealed class ShardNode : Node2D, IShard
{
    private readonly LateInit<Node2D> _entityRootLateInit = new();
    public Node2D EntityRoot => _entityRootLateInit;
    
    public Guid Id { get; set; }
    
    public override void _Ready()
    {
        _entityRootLateInit.Value = GetNode<Node2D>("Entities");
    }
}
