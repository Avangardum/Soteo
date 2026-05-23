using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Entities;

public sealed class UnitPuppet : UnitBase<IUnitPuppetNode>
{
    private readonly ICamera _camera;
    
    public UnitPuppet(Guid id, IUnitPuppetNode node, ICamera camera) : base(id, node)
    {
        _camera = camera;
        camera.ZoomChanged += OnZoomChanged;
    }

    public override void Remove()
    {
        base.Remove();
        _camera.ZoomChanged -= OnZoomChanged;
    }

    public override bool IsDead
    {
        get => base.IsDead;
        protected set
        {
            base.IsDead = value;
            Node?.IsAzimuthIndicatorVisible = !value;
        }
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
    
    public override double Azimuth
    {
        get => base.Azimuth;
        set
        {
            base.Azimuth = value;
            Node?.CalculateAzimuthIndicatorPoints(Azimuth, _camera.TrueZoom);
        }
    }
    
    private Dictionary<Guid, PuppetStatusContext> StatusesInternal { get; set; } = [];
    public IReadOnlyDictionary<Guid, PuppetStatusContext> Statuses => StatusesInternal;
    
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

    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        base.ReplicateSnapshot(snapshot);
        var s = (UnitPuppetSnapshot)snapshot;
        IsDead = s.IsDead;
        IsMoving = s.IsMoving;
        StatsInternal = s.Stats.ToDictionary();
        AbilitySlotStatesInternal = s.AbilitySlotStates.ToDictionary();
        AbilityUseProgress = s.AbilityUseProgress;
        StatusesInternal = s.Statuses.ToDictionary();
        UpdateAnimation();
    }

    public override void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight)
    {
        base.ApplyDelta(delta, interpolationWeight);
        var d = (UnitPuppetSnapshotDelta)delta;
        if (d.IsDead.HasChanged)
            IsDead = d.IsDead.NewValue;
        if (d.IsMoving.HasChanged)
            IsMoving = d.IsMoving.NewValue;
        d.Stats.MutateDictionary(StatsInternal, interpolationWeight, null);
        d.AbilitySlotStates.MutateDictionary(
            AbilitySlotStatesInternal, interpolationWeight, AbilitySlotState.Interpolate);
        if (d.AbilityUseProgress.HasChanged)
        {
            AbilityUseProgress = Maths.InterpolateNullableClass(
                AbilityUseProgress, d.AbilityUseProgress.NewValue, interpolationWeight, AbilityUseProgress.Interpolate);
        }
        d.Statuses.MutateDictionary(StatusesInternal, interpolationWeight, PuppetStatusContext.Interpolate);
        UpdateAnimation();
    }

    private void OnZoomChanged()
    {
        UpdateNodePosition();
        Node?.CalculateAzimuthIndicatorPoints(Azimuth, _camera.TrueZoom);
    }
    
    private void UpdateAnimation()
    {
        if (IsRemoved) return;
        
        Node.FlipSpriteH = Azimuth >= 180;
        
        if (IsDead)
        {
            Node.Animation = "Death";
            Node.AnimationSpeedScale = 1;
        }
        else if (AbilityUseProgress != null)
        {
            var ability = AbilitySlotStates[AbilityUseProgress.Slot].Ability;
            Node.Animation = ability.Animation;
            if (ability.LoopAnimation)
            {
                Node.AnimationSpeedScale = 1;
            }
            else
            {
                int frameCount = Node.AnimationFrameCount;
                double progress = AbilityUseProgress.NormalizedProgress;
                Node.AnimationFrame = Mathf.Min(Maths.FloorToInt(frameCount * progress), frameCount - 1);
                Node.AnimationSpeedScale = 0;
            }
        }
        else if (IsMoving)
        {
            Node.Animation = "Walk Right";
            const double referenceMoveSpeed = 35;
            Node.AnimationSpeedScale = (float)(Stats[Stat.MoveSpeed] / referenceMoveSpeed);
        }
        else
        {
            Node.Animation = "Idle Right";
            Node.AnimationSpeedScale = 1;
        }
    }
}
