using System.Numerics;
using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Deltas;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;
using Soteo.Util;

namespace Soteo.Core.Entities;

public sealed class UnitPuppet : UnitBase<IUnitPuppetNode>, IUnitPuppet
{
    private readonly ICamera _camera;
    private double _visualInventoryLockTimeRemaining;
    private List<Guid> _controllingPlayerIds = [];

    public Inventory Inventory { get; } = new(0);

    /// <inheritdoc/>
    public Inventory VisualInventory { get; } = new(0);

    public IReadOnlyList<Guid> ControllingPlayerIds => _controllingPlayerIds;

    public UnitPuppet(Guid id, IUnitPuppetNode node, ICamera camera) : base(id, node)
    {
        _camera = camera;
        camera.ZoomChanged += OnZoomChanged;
    }

    public override void Remove()
    {
        _camera.ZoomChanged -= OnZoomChanged;
        base.Remove();
    }

    public override bool IsDead
    {
        get => base.IsDead;
        protected set
        {
            base.IsDead = value;
            if (IsDead && Node != null)
                Node.IsAzimuthIndicatorVisible = false;
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
            Node?.CalculateAzimuthIndicatorPoints(Azimuth, _camera.Zoom);
        }
    }

    private Dictionary<Guid, PuppetStatusContext> StatusesInternal { get; set; } = [];
    public IReadOnlyDictionary<Guid, PuppetStatusContext> Statuses => StatusesInternal;

    private void UpdateNodePosition()
    {
        Node?.PositionMeters = NodeHelper.RoundPositionToPixelPerfect
        (
            Position,
            _camera.Zoom,
            isCamera: false,
            Node.HalfPixelXVisualOffset,
            Node.HalfPixelYVisualOffset
        );
    }

    public override EntitySnapshot ToSnapshot() => throw new NotSupportedException();

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
        Inventory.ReplicateSnapshot(s.Items);
        if (_visualInventoryLockTimeRemaining == 0)
            VisualInventory.ReplicateSnapshot(s.Items);
        _controllingPlayerIds = s.ControllingPlayerIds.ToList();
        EquipmentSlotStatesInternal = s.EquipmentSlotStates.ToDictionary();
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
        d.Stats.MutateDictionary(StatsInternal);
        d.AbilitySlotStates.MutateDictionary
        (
            AbilitySlotStatesInternal,
            interpolationWeight,
            AbilitySlotState.Interpolate
        );
        if (d.AbilityUseProgress.HasChanged)
        {
            AbilityUseProgress = Maths.InterpolateNullableClass
            (
                AbilityUseProgress,
                d.AbilityUseProgress.NewValue,
                interpolationWeight,
                AbilityUseProgress.Interpolate
            );
        }
        d.Statuses.MutateDictionary(StatusesInternal, interpolationWeight, PuppetStatusContext.Interpolate);
        Inventory.ApplyDelta(d.Items);
        if (_visualInventoryLockTimeRemaining == 0)
            VisualInventory.ApplyDelta(d.Items);
        d.ControllingPlayerIds.MutateList(_controllingPlayerIds);
        d.EquipmentSlotStates.MutateDictionary(EquipmentSlotStatesInternal);
        UpdateAnimation();
    }

    private void OnZoomChanged()
    {
        UpdateNodePosition();
        Node?.CalculateAzimuthIndicatorPoints(Azimuth, _camera.Zoom);
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
            UpdateAbilityAnimation(Node, AbilityUseProgress);
        }
        else if (IsMoving)
        {
            Node.Animation = "Move";
            const double referenceMoveSpeed = 3.5;
            Node.AnimationSpeedScale = (float)(Stats[Stat.MoveSpeed] / referenceMoveSpeed);
        }
        else
        {
            Node.Animation = "Idle";
            Node.AnimationSpeedScale = 1;
        }
    }

    private void UpdateAbilityAnimation(IUnitPuppetNode node, AbilityUseProgress abilityUseProgress)
    {
        Ability ability = AbilitySlotStates[abilityUseProgress.Slot].Ability;
        node.Animation = ability.Animation;
        if (ability.LoopAnimation)
        {
            node.AnimationSpeedScale = 1;
        }
        else
        {
            int frameCount = node.AnimationFrameCount;
            double progress = abilityUseProgress.NormalizedProgress;
            node.AnimationFrame = Maths.Min(Maths.FloorToInt(frameCount * progress), frameCount - 1);
            node.AnimationSpeedScale = 0;
        }
    }

    public bool IsAlliedTo(IUnitPuppet other) => Faction != Faction.Neutral && other.Faction == Faction;

    /// <inheritdoc/>
    public void LockVisualInventory()
    {
        _visualInventoryLockTimeRemaining = 0.5;
    }

    public void Process(double delta)
    {
        double prevVisualInventoryLockTimeRemaining = _visualInventoryLockTimeRemaining;
        _visualInventoryLockTimeRemaining = Math.Max(_visualInventoryLockTimeRemaining - delta, 0);
        if (prevVisualInventoryLockTimeRemaining > 0 && _visualInventoryLockTimeRemaining == 0)
            VisualInventory.ReplicateSnapshot(Inventory.CreateSnapshot());
    }
}
