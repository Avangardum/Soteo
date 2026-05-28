using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Util;

namespace Soteo.Gameplay.EntityNodes;

public sealed class UnitPuppetNode : Node2D, IDeferredRemovalEntityNode, IUnitPuppetNode
{
    private double? _removalCountdown;
    private TaskCompletionSource _waitUntilCanRemoveSource = new();
    
    private readonly LateInit<AnimatedSprite> _sprite = new();
    private readonly LateInit<AzimuthIndicator> _azimuthIndicator = new();
    
    private AnimatedSprite Sprite => _sprite;
    private AzimuthIndicator AzimuthIndicator => _azimuthIndicator;
    
    // If the sprite has position with .5 as fractional part in any dimension (used to center sprites with odd sizes),
    // the following fields help compensate it for pixel perfect rendering. See NodeHelper for details.
    [Export] private bool _halfPixelXVisualOffset;
    [Export] private bool _halfPixelYVisualOffset;
    
    public bool HalfPixelXVisualOffset => _halfPixelXVisualOffset;
    public bool HalfPixelYVisualOffset => _halfPixelYVisualOffset;
    
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
        AzimuthIndicator.Visible = true;
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
        _sprite.Value = GetNode<AnimatedSprite>("AnimatedSprite");
        _azimuthIndicator.Value = GetNode<AzimuthIndicator>("AzimuthIndicator");
        
        Sprite.Playing = true;
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
        get => AzimuthIndicator.Visible;
        set => AzimuthIndicator.Visible = value;
    }

    public bool FlipSpriteH
    {
        get => Sprite.FlipH;
        set => Sprite.FlipH = value;
    }

    public string Animation
    {
        get => Sprite.Animation;
        set => Sprite.Animation = value;
    }

    public double AnimationSpeedScale
    {
        get => Sprite.SpeedScale;
        set => Sprite.SpeedScale = (float)value;
    }

    public int AnimationFrame
    {
        get => Sprite.Frame;
        set => Sprite.Frame = value;
    }

    public int AnimationFrameCount => Sprite.Frames.GetFrameCount(Animation);

    public void CalculateAzimuthIndicatorPoints(double azimuth, double zoom) =>
        AzimuthIndicator.CalculatePoints(azimuth, zoom);
}
