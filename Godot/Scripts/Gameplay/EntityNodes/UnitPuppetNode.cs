using Soteo.Core.Entities;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;
using Soteo.Main.Gameplay.Interfaces;
using Soteo.Util;

namespace Soteo.Main.Gameplay.EntityNodes;

public sealed class UnitPuppetNode : Node2D, IDeferredRemovalEntityNode, IUnitPuppetNode
{
    private double? _removalCountdown;
    private TaskCompletionSource _waitUntilCanRemoveSource = new();

    private readonly LateInit<AnimatedSprite2D> _sprite = new();
    private readonly LateInit<AzimuthIndicator> _azimuthIndicator = new();

    private AnimatedSprite2D Sprite2D => _sprite;
    private AzimuthIndicator AzimuthIndicator => _azimuthIndicator;

    // If a sprite has pixel position with .5 as the fractional part in any dimension
    // (used to center sprites with odd sizes), the following fields help compensate
    // it for pixel perfect rendering. See NodeHelper for details.
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

    public Vector2 PositionMeters
    {
        get => Position.ToSys() / Const.PixelsInMeter;
        set => Position = value.ToGd() * Const.PixelsInMeter;
    }

    public override void _Ready()
    {
        _sprite.Value = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _azimuthIndicator.Value = GetNode<AzimuthIndicator>("AzimuthIndicator");

        Sprite2D.Playing = true;
    }

    public override void _Process(float delta)
    {
        UnitPuppet?.Process(delta);

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
        get => Sprite2D.FlipH;
        set => Sprite2D.FlipH = value;
    }

    public string Animation
    {
        get => Sprite2D.Animation;
        set => Sprite2D.Animation = value;
    }

    public double AnimationSpeedScale
    {
        get => Sprite2D.SpeedScale;
        set => Sprite2D.SpeedScale = (float)value;
    }

    public int AnimationFrame
    {
        get => Sprite2D.Frame;
        set => Sprite2D.Frame = value;
    }

    public int AnimationFrameCount => Sprite2D.Frames.GetFrameCount(Animation);

    public void CalculateAzimuthIndicatorPoints(double azimuth, double zoom) =>
        AzimuthIndicator.CalculatePoints(azimuth, zoom);
}
