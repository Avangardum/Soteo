using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Entities;

public sealed class ProjectilePuppet : Entity<ProjectilePuppetNode>
{
    private readonly ICamera _camera;

    public ProjectilePuppet(Guid id, ProjectilePuppetNode node, ICamera camera) : base(id, node)
    {
        _camera = camera;
        camera.ZoomChanged += OnZoomChanged;
    }

    public override void Remove()
    {
        base.Remove();
        _camera.ZoomChanged -= OnZoomChanged;
    }

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
        Node?.Position = NodeHelper.RoundPositionToPixelPerfect
        (
            Position,
            _camera,
            isCamera: false,
            Node.Properties.HalfPixelXVisualOffset,
            Node.Properties.HalfPixelYVisualOffset
        );
    }
    
    public override EntitySnapshot CreateSnapshot() => throw new InvalidOperationException();
    
    private void OnZoomChanged()
    {
        UpdateNodePosition();
    }
}
