using System.Diagnostics.CodeAnalysis;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Entities;

public abstract class Entity<TNode> : IEntity where TNode : Node2D
{
    protected Entity(Guid id, ClientDependency<ICamera> camera)
    {
        Id = id;
        Camera = camera;
        Camera.Value?.ZoomChanged += OnZoomChanged;
    }

    public event Action Removed = delegate {};

    protected ClientDependency<ICamera> Camera { get; }
    [MemberNotNullWhen(false, nameof(Node))] public abstract bool IsRemoved { get; protected set; }
    protected abstract TNode? Node { get; }
    public Guid Id { get; }
    public abstract Vector2 Position { get; set; }
    public virtual double Azimuth { get; set => field = SoteoMath.PosMod(value, 360); }

    public abstract EntitySnapshot CreateSnapshot();

    public abstract void ReplicateSnapshot(EntitySnapshot snapshot);

    public void Remove()
    {
        if (IsRemoved) return;
        IsRemoved = true;
        Camera.Value?.ZoomChanged -= OnZoomChanged;
        Node.QueueFree();
        Removed();
    }
    
    protected abstract void OnZoomChanged();
    
    /// <summary>
    /// Round a visual position value to a value that will allow pixel perfect rendering without artifacts due to
    /// the sprite's pixels having noninteger position, therefore rendering between screen pixels
    /// </summary>
    /// <param name="value">Value to round</param>
    /// <param name="camera">Camera</param>
    /// <param name="halfPixelXOffset">
    /// Whether a half screen pixel x offset should be applied. Use when the sprite's x position ends in .5
    /// </param>
    /// <param name="halfPixelYOffset">
    /// Whether a half screen pixel y offset should be applied. Use when the sprite's y position ends in .5
    /// </param>
    public Vector2 RoundVisualPositionToPixelPerfect
    (
        Vector2 value,
        bool halfPixelXOffset,
        bool halfPixelYOffset
    )
    {
        if (Camera.Value == null) return value;
        
        // If zoom is even, a world pixel with half pixel offset will be rendered as even number of screen pixels,
        // which will distribute equally in all directions, so pixel perfect rendering is possible without having
        // to compensate for this offset.
        if (SoteoMath.IsMultipleOf(2, Camera.Value.TrueZoom))
            halfPixelXOffset = halfPixelYOffset = false;
        
        double screenPixelSizeInWorldPixels = 1 / Camera.Value.TrueZoom;
        double roundedX = halfPixelXOffset ? SoteoMath.RoundToMultipleOfPlusHalf(screenPixelSizeInWorldPixels, value.x) :
            SoteoMath.RoundToMultipleOf(screenPixelSizeInWorldPixels, value.x);
        double roundedY = halfPixelYOffset ? SoteoMath.RoundToMultipleOfPlusHalf(screenPixelSizeInWorldPixels, value.y) :
            SoteoMath.RoundToMultipleOf(screenPixelSizeInWorldPixels, value.y);
        return Vector2.New(roundedX, roundedY);
    }
}