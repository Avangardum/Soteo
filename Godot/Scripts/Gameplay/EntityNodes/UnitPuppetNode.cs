using Soteo.Core;
using Soteo.Core.Entities;
using Soteo.Core.Entities.Units;

namespace Soteo.Main.Gameplay.EntityNodes;

public sealed partial class UnitPuppetNode : Node2D, IDeferredRemovalEntityNode, IUnitPuppetNode
{
    private double? _removalCountdown;
    private TaskCompletionSource _waitUntilCanRemoveSource = new();

    private AnimatedSprite2D _sprite = null!;
    private AzimuthIndicator _azimuthIndicator = null!;

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
        _azimuthIndicator.Visible = true;
    }

    private void OnDetached()
    {
        _removalCountdown = 5;
    }

    public Entity? Entity
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
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _azimuthIndicator = GetNode<AzimuthIndicator>("AzimuthIndicator");

        _sprite.Play();
    }

    public override void _Process(double delta)
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

    public int AnimationFrameCount => _sprite.SpriteFrames.GetFrameCount(Animation);

    public void CalculateAzimuthIndicatorPoints(double azimuth, double zoom) =>
        _azimuthIndicator.CalculatePoints(azimuth, zoom);
}
