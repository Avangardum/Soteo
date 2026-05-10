using System.Collections.Immutable;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.EntityNodes;
using Soteo.Gameplay.Util;
using Soteo.Shared;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Entities;

public sealed class UnitPuppet : UnitBase<UnitPuppetNode>
{
    public UnitPuppet(Guid id, UnitPuppetNode node, IServiceProvider serviceProvider) :
        base(id, node, serviceProvider) { }
    
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
            Node?.AzimuthIndicator.CalculatePoints(Azimuth, Camera.Required.TrueZoom);
        }
    }
    
    private Dictionary<Guid, PuppetStatusContext> StatusesInternal { get; set; } = [];
    public IReadOnlyDictionary<Guid, PuppetStatusContext> Statuses => StatusesInternal;
    
    private void UpdateNodePosition()
    {
        Node?.Position = NodeHelper.RoundPositionToPixelPerfect
        (
            Position,
            Camera.Value,
            isCamera: false,
            Node.Properties.HalfPixelXVisualOffset,
            Node.Properties.HalfPixelYVisualOffset
        );
    }

    public override EntitySnapshot CreateSnapshot() => throw new InvalidOperationException();

    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        base.ReplicateSnapshot(snapshot);
        var s = (UnitPuppetSnapshot)snapshot;
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

    protected override void OnZoomChanged()
    {
        UpdateNodePosition();
    }
    
    private void UpdateAnimation()
    {
        if (IsRemoved) return;
        
        Node.Sprite.FlipH = Azimuth >= 180;
        
        if (AbilityUseProgress != null)
        {
            var ability = AbilitySlotStates[AbilityUseProgress.Slot].Ability;
            Node.Sprite.Animation = ability.Animation;
            if (ability.LoopAnimation)
            {
                Node.Sprite.Animation = ability.Animation;
                Node.Sprite.SpeedScale = 1;
            }
            else
            {
                int frameCount = Node.Sprite.Frames.GetFrameCount(ability.Animation);
                double progress = AbilityUseProgress.NormalizedProgress;
                Node.Sprite.Frame = Mathf.Min(Maths.FloorToInt(frameCount * progress), frameCount - 1);
                Node.Sprite.SpeedScale = 0;
            }
        }
        else if (IsMoving)
        {
            Node.Sprite.Animation = "Walk Right";
            const double referenceMoveSpeed = 35;
            Node.Sprite.SpeedScale = (float)(Stats[Stat.MoveSpeed] / referenceMoveSpeed);
        }
        else
        {
            Node.Sprite.Animation = "Idle Right";
            Node.Sprite.SpeedScale = 1;
        }
    }
}