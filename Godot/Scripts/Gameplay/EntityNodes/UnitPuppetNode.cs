using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;

namespace Soteo.Gameplay.EntityNodes;

public sealed class UnitPuppetNode : Node2D, IDeferredRemovalEntityNode, IUnitPuppetNode
{
    private double? _removalCountdown;
    private TaskCompletionSource _waitUntilCanRemoveSource = new();
    
    public Node2D Node => this;
    
    public UnitPuppet? UnitPuppet
    {
        get;
        set
        {
            field = value;
            
            if (value != null)
            {
                _removalCountdown = null;
                _waitUntilCanRemoveSource = new TaskCompletionSource();
            }
            else
            {
                _removalCountdown = 5;
            }
        }
    }
    
    public IEntity? Entity
    {
        get => UnitPuppet;
        set => UnitPuppet = (UnitPuppet?)value;
    }
    
    public AnimatedSprite Sprite { get; private set; } = null!;
    public AzimuthIndicator AzimuthIndicator { get; private set; } = null!;
    public EntityProperties Properties { get; private set; } = null!;
    
    public override void _Ready()
    {
        Sprite = GetNode<AnimatedSprite>("Visuals/AnimatedSprite");
        AzimuthIndicator = GetNode<AzimuthIndicator>("Visuals/AzimuthIndicator");
        Properties = GetNode<EntityProperties>("Properties");
        
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

    public bool HalfPixelXVisualOffset => Properties.HalfPixelXVisualOffset;

    public bool HalfPixelYVisualOffset => Properties.HalfPixelYVisualOffset;

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
