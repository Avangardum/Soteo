using System.Numerics;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Core.Entities;

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
            _camera.Zoom,
            isCamera: false,
            Node.HalfPixelXVisualOffset,
            Node.HalfPixelYVisualOffset
        );
    }
    
    public override EntitySnapshot ToSnapshot() => throw new NotSupportedException();
    
    private void OnZoomChanged() => UpdateNodePosition();
}
