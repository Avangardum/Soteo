using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Entities;

public sealed class ProjectilePuppet : Entity<IProjectilePuppetNode>
{
    private readonly ICamera _camera;

    public ProjectilePuppet(Guid id, IProjectilePuppetNode node, ICamera camera) : base(id, node)
    {
        _camera = camera;
        camera.ZoomChanged += OnZoomChanged;
    }

    public override void Remove()
    {
        _camera.ZoomChanged -= OnZoomChanged;
        base.Remove();
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
            Node.HalfPixelXVisualOffset,
            Node.HalfPixelYVisualOffset
        );
    }
    
    public override EntitySnapshot CreateSnapshot() => throw new InvalidOperationException();
    
    private void OnZoomChanged()
    {
        UpdateNodePosition();
    }
}
