using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;

namespace Soteo.Gameplay.EntityNodes;

public sealed class UnitPuppetNode : Node2D, IDeferredRemovalEntityNode, IUnitPuppetNode
{
    private double? _removalCountdown;
    private TaskCompletionSource _waitUntilCanRemoveSource = new();
    
    private AnimatedSprite _sprite = null!;
    private AzimuthIndicator _azimuthIndicator = null!;
    private EntityProperties _properties = null!;
    
    public UnitPuppet? UnitPuppet
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            if (value != null)
                OnAttached();
            else
                OnDetached();
        }
    }
    
    private void OnAttached()
    {
        _removalCountdown = null;
        _waitUntilCanRemoveSource = new TaskCompletionSource();
        _azimuthIndicator.Visible = true;
    }
    
    private void OnDetached()
    {
        _removalCountdown = 5;
    }
    
    public IEntity? Entity
    {
        get => UnitPuppet;
        set => UnitPuppet = (UnitPuppet?)value;
    }
    
    public new Vector2 Position
    {
        get => base.Position.ToSys();
        set => base.Position = value.ToGd();
    }
    
    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite>("Visuals/AnimatedSprite");
        _azimuthIndicator = GetNode<AzimuthIndicator>("Visuals/AzimuthIndicator");
        _properties = GetNode<EntityProperties>("Properties");
        
        _sprite.Playing = true;
    }

    public override void _Process(float delta)
    {
        _removalCountdown -= delta;
        if (_removalCountdown <= 0)
        {
            _removalCountdown = null;
            _waitUntilCanRemoveSource.SetResult();
        }
    }

    public async Task WaitUntilCanRemoveAsync() => await _waitUntilCanRemoveSource.Task;

    public bool IsAzimuthIndicatorVisible
    {
        get => _azimuthIndicator.Visible;
        set => _azimuthIndicator.Visible = value;
    }

    public bool HalfPixelXVisualOffset => _properties.HalfPixelXVisualOffset;
    public bool HalfPixelYVisualOffset => _properties.HalfPixelYVisualOffset;

    public bool FlipSpriteH
    {
        get => _sprite.FlipH;
        set => _sprite.FlipH = value;
    }

    public string Animation
    {
        get => _sprite.Animation;
        set => _sprite.Animation = value;
    }

    public double AnimationSpeedScale
    {
        get => _sprite.SpeedScale;
        set => _sprite.SpeedScale = (float)value;
    }

    public int AnimationFrame
    {
        get => _sprite.Frame;
        set => _sprite.Frame = value;
    }

    public int AnimationFrameCount => _sprite.Frames.GetFrameCount(Animation);

    public void CalculateAzimuthIndicatorPoints(double azimuth, double zoom) =>
        _azimuthIndicator.CalculatePoints(azimuth, zoom);
}
