using System.Diagnostics.CodeAnalysis;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared;

namespace Soteo.Gameplay.Entities;

public abstract class Entity<TNode> : IEntity where TNode : Node2D, IEntityNode
{
    protected Entity(Guid id, TNode node, ClientDependency<ICamera> camera)
    {
        Id = id;
        
        Node = node;
        node.Entity = this;
        
        Camera = camera;
        Camera.Value?.ZoomChanged += OnZoomChanged;
    }

    // Both
    public event Action Removed = delegate {};

    // Client
    protected ClientDependency<ICamera> Camera { get; }
    // Both
    [MemberNotNullWhen(false, nameof(Node))] public bool IsRemoved { get; private set; }
    // Both
    protected TNode? Node { get; private set; }
    // Both
    public Guid Id { get; }
    // Both
    public virtual Vector2 Position { get; set; }
    // Both
    public virtual double Azimuth { get; set => field = Maths.PosMod(value, 360); }

    // Server
    public abstract EntitySnapshot CreateSnapshot();

    // Both
    public virtual void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        Position = snapshot.Position;
        Azimuth = snapshot.Azimuth;
    }

    // Both
    public void Remove()
    {
        if (IsRemoved) return;
        IsRemoved = true;
        Camera.Value?.ZoomChanged -= OnZoomChanged;
        Node.Entity = null;
        Node = null;
        Removed();
    }
    
    // Client
    protected virtual void OnZoomChanged() { }
    
    // Client
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
        if (Maths.IsMultipleOf(2, Camera.Value.TrueZoom))
            halfPixelXOffset = halfPixelYOffset = false;
        
        double screenPixelSizeInWorldPixels = 1 / Camera.Value.TrueZoom;
        double roundedX = halfPixelXOffset ? Maths.RoundToMultipleOfPlusHalf(screenPixelSizeInWorldPixels, value.x) :
            Maths.RoundToMultipleOf(screenPixelSizeInWorldPixels, value.x);
        double roundedY = halfPixelYOffset ? Maths.RoundToMultipleOfPlusHalf(screenPixelSizeInWorldPixels, value.y) :
            Maths.RoundToMultipleOf(screenPixelSizeInWorldPixels, value.y);
        return Vector2.New(roundedX, roundedY);
    }
}