using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Entities;

public sealed class ProjectilePuppet : Entity<ProjectilePuppetNode>
{
    public ProjectilePuppet(Guid id, ProjectilePuppetNode node, ICamera camera) :
        base(id, node, ClientDependency.From(camera)) { }
    
    public override Vector2 Position
    {
        get => base.Position;
        set
        {
            base.Position = value;
            UpdateNodePosition();
        }
    }
    
    private void UpdateNodePosition()
    {
        Node?.Position = RoundVisualPositionToPixelPerfect
        (
            Position,
            Node.Properties.HalfPixelXVisualOffset,
            Node.Properties.HalfPixelYVisualOffset
        );
    }
    
    public override EntitySnapshot CreateSnapshot() => throw new InvalidOperationException();
    
    protected override void OnZoomChanged()
    {
        UpdateNodePosition();
    }
}