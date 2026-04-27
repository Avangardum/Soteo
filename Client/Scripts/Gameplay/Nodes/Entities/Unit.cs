using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public abstract class Unit : Entity<UnitNode>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityManager _entityManager;
    
    private bool _isMoving;
    
    protected Unit(Guid id, PackedScene scene, IServiceProvider serviceProvider) :
        base(id, serviceProvider.GetRequiredService<ClientDependency<ICamera>>())
    {
        _serviceProvider = serviceProvider;
        _entityManager = serviceProvider.GetRequiredService<IEntityManager>();
        
        Node = new UnitNode(this, scene, serviceProvider.GetRequiredService<IShard>());
        Node.Name = $"{GetType().Name} {id}";
        
        foreach (Stat stat in Enum.GetValues<Stat>()) StatsInternal[stat] = DefaultStats[stat];
        
        Faction = Id.GetHashCode() % 2 == 0 ? Faction.Empire : Faction.Syndicate;
    }
    
    public static readonly IReadOnlyDictionary<Stat, float> DefaultStats = new Dictionary<Stat, float>
    {
        [Stat.MaxHealth] = 1000,
        [Stat.CurrentHealth] = 1000,
        [Stat.MaxMana] = 1000,
        [Stat.CurrentMana] = 1000,
        [Stat.MoveSpeed] = 50,
        [Stat.TurnSpeed] = 360,
        [Stat.AttackDamage] = 50,
        [Stat.AttackSpeed] = 1000,
        [Stat.AttackUseTimeFraction] = 0.5f,
        [Stat.AttackRange] = 100,
        [Stat.AttackProjectileSpeed] = 500
    }.ToImmutableDictionary(); 
    
    private Queue<ICommand> Commands { get; } = [];
    
    private Dictionary<Stat, float> StatsInternal { get; set; } = [];
    public IReadOnlyDictionary<Stat, float> Stats => StatsInternal;
    
    protected Dictionary<AbilitySlot, AbilityState> AbilityStatesInternal { get; set; } = [];
    public IReadOnlyDictionary<AbilitySlot, AbilityState> AbilityStates => AbilityStatesInternal;

    public AbilityUseProgress? AbilityUseProgress { get; private set; }
    [MemberNotNullWhen(false, nameof(Node))] public override bool IsRemoved { get; protected set; }
    protected override UnitNode? Node => field.AsValid();
    public Faction Faction { get; set; }

    public override Vector2 Position
    {
        get;
        set
        {
            field = value;
            if (IsRemoved) return;
            if (IsServer) Node.Position = Position;
            else Node.Visuals.Position = RoundVisualPositionToPixelPerfect(Position,
                Node.Properties.HalfPixelXVisualOffset, Node.Properties.HalfPixelYVisualOffset) - Node.Position;
        }
    }
    
    public override float Azimuth
    {
        get => base.Azimuth;
        set
        {
            base.Azimuth = value;
            if (IsRemoved) return;
            Node.AzimuthLine.RotationDegrees = value;
        }
    }
    
    public override EntitySnapshot CreateSnapshot()
    {
        return new UnitSnapshot
        {
            Id = Id,
            Position = Position,
            Azimuth = Azimuth,
            IsMoving = _isMoving,
            Stats = Stats.ToImmutableDictionary(),
            AbilityStates = AbilityStatesInternal.ToImmutableDictionary(),
            AbilityUseProgress = AbilityUseProgress
        };
    }

    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        var s = (UnitSnapshot)snapshot;
        
        Position = s.Position;
        Azimuth = s.Azimuth;
        _isMoving = s.IsMoving;
        StatsInternal = s.Stats.ToDictionary();
        AbilityStatesInternal = s.AbilityStates.ToDictionary();
        AbilityUseProgress = s.AbilityUseProgress;
        
        UpdateAnimation();
    }

    protected override void OnZoomChanged()
    {
        // Trigger Position setter to recalculate position of visuals
        Position = Position;
    }

    public virtual void _PhysicsProcessServer(UnitNode node, float delta)
    {
        _isMoving = false;
        DecreaseCooldowns(delta);
        ExecuteCommands(node, delta);
    }
    
    private void DecreaseCooldowns(float delta)
    {
        foreach (AbilitySlot slot in AbilityStatesInternal.Keys.ToList())
        {
            AbilityStatesInternal[slot] = AbilityStatesInternal[slot] with
            {
                Cooldown = Mathf.Max(AbilityStatesInternal[slot].Cooldown - delta, 0)
            };
        }
    }

    public virtual void _PhysicsProcessClient(UnitNode node, float delta)
    {
        node.Position = Position;
        node.Visuals.Position = Vector2.Zero;
    }

    private void ExecuteCommands(UnitNode node, float deltaTime)
    {
        float remainingDeltaTime = deltaTime;
        int iterations = 0;
        const int maxIterations = 5;
        while (Commands.Count > 0 && remainingDeltaTime > 0 && iterations < maxIterations)
        {
            iterations++;
            switch (Commands.Peek())
            {
                case LookCommand command:
                    LookAtPosition(command.Position, ref remainingDeltaTime);
                    break;
                case MoveCommand command:
                    MoveToPosition(command.Position, ref remainingDeltaTime, node);
                    break;
                case UseAbilityCommand command:
                    UseAbility(command, ref remainingDeltaTime, node);
                    break;
            }
        }
    }
    
    private void LookAtPosition(Vector2 position, ref float remainingDeltaTime)
    {
        LookInDirection(position - Position, ref remainingDeltaTime);
    }
    
    private void LookInDirection(Vector2 direction, ref float remainingDeltaTime)
    {
        LookAtAzimuth(SoteoMath.DirectionToAzimuth(direction), ref remainingDeltaTime);
    }
    
    private void LookAtAzimuth(float azimuth, ref float remainingDeltaTime)
    {
        if (remainingDeltaTime == 0 || Stats[Stat.TurnSpeed] == 0) return;
        
        float desiredDeltaAzimuth = SoteoMath.ModularDelta(Azimuth, azimuth, 360);
        
        float timeToComplete = Mathf.Abs(desiredDeltaAzimuth) / Stats[Stat.TurnSpeed];
        if (timeToComplete <= remainingDeltaTime)
        {
            Azimuth += desiredDeltaAzimuth;
            remainingDeltaTime -= timeToComplete;
            if (Commands.PeekOrDefault() is LookCommand) Commands.Dequeue();
        }
        else
        {
            Azimuth += Mathf.Sign(desiredDeltaAzimuth) * remainingDeltaTime * Stats[Stat.TurnSpeed];
            remainingDeltaTime = 0;
        }
    }
    
    private void MoveToPosition(Vector2 position, ref float remainingDeltaTime, UnitNode node)
    {
        LookAtPosition(position, ref remainingDeltaTime);
        if (remainingDeltaTime == 0 || Stats[Stat.MoveSpeed] == 0) return;
        
        Vector2 desiredMovement = position - Position;
        float desiredMovementLength = desiredMovement.Length();
        if (desiredMovementLength == 0)
        {
            if (Commands.PeekOrDefault() is MoveCommand) Commands.Dequeue();
            return;
        }
        Vector2 normalizedDesiredMovement = desiredMovement / desiredMovementLength;
        float timeToComplete = desiredMovementLength / Stats[Stat.MoveSpeed];
        if (timeToComplete <= remainingDeltaTime)
        {
            MoveAndCollide(desiredMovement, node);
            remainingDeltaTime -= timeToComplete;
            if (Commands.PeekOrDefault() is MoveCommand) Commands.Dequeue();
        }
        else
        {
            Vector2 movement = normalizedDesiredMovement * Stats[Stat.MoveSpeed] * remainingDeltaTime;
            MoveAndCollide(movement, node);
            remainingDeltaTime = 0;
        }
        _isMoving = true;
    }
    
    private KinematicCollision2D MoveAndCollide(Vector2 movement, UnitNode node)
    {
        KinematicCollision2D collision = node.MoveAndCollide(movement);
        Position = node.Position;
        return collision;
    }

    private void UseAbility(UseAbilityCommand command, ref float remainingDeltaTime, UnitNode node)
    {
        if (!AbilityStatesInternal.TryGetValue(command.Slot, out AbilityState? state))
        {
            Commands.Dequeue();
            AbilityUseProgress = null;
            return;
        }
        
        AbilityContext context = GetAbilityContext(command);
        
        if (state.Cooldown > 0)
        {
            if (command.Repeat)
                WaitForAbilityCooldown(context, ref remainingDeltaTime);
            else
                Commands.Dequeue();
            AbilityUseProgress = null;
            return;
        }
        
        AbilityValidationResult validationResult =
            ValidateAbilityWithCorrection(state.Ability, context, command, ref remainingDeltaTime, node);
        if (validationResult != AbilityValidationResult.Ok || remainingDeltaTime == 0)
        {
            AbilityUseProgress = null;
            return;
        }

        if (AbilityUseProgress?.Slot != command.Slot)
        {
            AbilityUseProgress = new AbilityUseProgress
            {
                Slot = command.Slot,
                RemainingTime = state.Ability.UseTime(context)
            };
        }
        
        if (remainingDeltaTime < AbilityUseProgress.RemainingTime)
        {
            AbilityUseProgress = AbilityUseProgress.AddTime(remainingDeltaTime);
            remainingDeltaTime = 0;
        }
        else
        {
            remainingDeltaTime -= AbilityUseProgress.RemainingTime;
            TriggerAbilityEffect(context, command);
        }
    }
    
    private void TriggerAbilityEffect(AbilityContext context, UseAbilityCommand command)
    {
        context.Ability.TakeEffect(context);
        float cooldown = context.Ability.Cooldown(context);
        AbilityStatesInternal[command.Slot] = AbilityStatesInternal[command.Slot] with
        {
            Cooldown = cooldown,
            MaxCooldown = cooldown
        };
        AbilityUseProgress = null;
        if (!command.Repeat)
            Commands.Dequeue();
    }
    
    private void WaitForAbilityCooldown(AbilityContext context, ref float remainingDeltaTime)
    {
        Vector2? targetPosition = context.TargetPosition ?? context.TargetUnit?.Position;
        if (targetPosition != null) LookAtPosition(targetPosition.Value, ref remainingDeltaTime);
        remainingDeltaTime = 0;
    }
    
    public AbilityContext GetAbilityContext(UseAbilityCommand command)
    {
        if (!AbilityStates.TryGetValue(command.Slot, out AbilityState? state))
            throw new ArgumentException($"Unit {Id} doesn't have an ability in slot {command.Slot}");
        return new AbilityContext
        {
            Ability = state.Ability,
            Level = state.Level,
            User = this,
            UserStats = Stats.ToImmutableDictionary(),
            ServiceProvider = _serviceProvider,
            TargetPosition = command.TargetPosition,
            TargetUnit = command.TargetUnitId == null ? null :
                _entityManager.GetEntity(command.TargetUnitId.Value) as Unit,
            TargetDirection = command.TargetDirection,
            TargetShardId = command.TargetShardId
        };
    }
    
    /// <summary>
    /// Validate an ability and if validation fails, try to make it pass
    /// </summary>
    private AbilityValidationResult ValidateAbilityWithCorrection
    (
        Ability ability,
        AbilityContext context,
        UseAbilityCommand command,
        ref float remainingDeltaTime,
        UnitNode node
    )
    {
        Vector2? targetPosition = context.TargetUnit?.Position ?? context.TargetPosition;
        AbilityValidationResult abilityValidationResult;
        int iterations = 0;
        const int maxIterations = 5;
        do
        {
            iterations++;
            abilityValidationResult = ability.Validate(context);
            switch (abilityValidationResult)
            {
                case AbilityValidationResult.Ok:
                    return abilityValidationResult;
                case AbilityValidationResult.OutOfRange:
                    MoveToPosition(targetPosition!.Value, ref remainingDeltaTime, node);
                    break;
                case AbilityValidationResult.OutOfAngularRange:
                    LookAtPosition(targetPosition!.Value, ref remainingDeltaTime);
                    break;
                default:
                    if (command.Repeat) remainingDeltaTime = 0;
                    else Commands.Dequeue();
                    return abilityValidationResult;
            }
        } while (remainingDeltaTime > 0 && iterations < maxIterations);
        return abilityValidationResult;
    }
    
    private void UpdateAnimation()
    {
        if (IsRemoved) return;
        Node.Sprite.FlipH = Azimuth >= 180;
        
        if (AbilityUseProgress != null)
        {
            var ability = AbilityStates[AbilityUseProgress.Slot].Ability;
            Node.Sprite.Animation = ability.Animation;
            if (ability.LoopAnimation)
            {
                Node.Sprite.Animation = ability.Animation;
                Node.Sprite.SpeedScale = 1;
            }
            else
            {
                int frameCount = Node.Sprite.Frames.GetFrameCount(ability.Animation);
                float progress = AbilityUseProgress.NormalizedProgress;
                Node.Sprite.Frame = Mathf.Min(Mathf.FloorToInt(frameCount * progress), frameCount - 1);
                Node.Sprite.SpeedScale = 0;
            }
        }
        else if (_isMoving)
        {
            Node.Sprite.Animation = "Walk Right";
            const float referenceMoveSpeed = 35;
            Node.Sprite.SpeedScale = Stats[Stat.MoveSpeed] / referenceMoveSpeed;
        }
        else
        {
            Node.Sprite.Animation = "Idle Right";
            Node.Sprite.SpeedScale = 1;
        }
    }

    public void SetCommand(ICommand command)
    {
        Commands.Clear();
        Commands.Enqueue(command);
        if (command is not UseAbilityCommand useAbilityCommand || useAbilityCommand.Slot != AbilityUseProgress?.Slot)
            AbilityUseProgress = null;
    }
     
    public void CancelCommands()
    {
        Commands.Clear();
    }
    
    public bool IsAlliedTo(Unit other) => Faction != Faction.Neutral && other.Faction == Faction;
    
    public void SpendHealth(float amount, Ability ability)
    {
        ChangeStat(Stat.CurrentHealth, -amount);
    }
    
    public void SpendMana(float amount, Ability ability)
    {
        ChangeStat(Stat.CurrentMana, -amount);
    }
    
    public void TakeDamage(float amount, Unit source, Ability ability)
    {
        ChangeStat(Stat.CurrentHealth, -amount);
    }
    
    public void RestoreHealth(float amount, Unit source, Ability ability)
    {
        ChangeStat(Stat.CurrentHealth, amount);
    }
    
    public void RestoreMana(float amount, Unit source, Ability ability)
    {
        ChangeStat(Stat.CurrentMana, amount);
    }
    
    protected void ChangeStat(Stat stat, float delta) => SetStat(stat, Stats[stat] + delta);

    protected void SetStat(Stat stat, float value)
    {
        float min = 0;
        float max = stat switch
        {
            Stat.CurrentHealth => Stats[Stat.MaxHealth],
            Stat.CurrentMana => Stats[Stat.MaxMana],
            _ => float.PositiveInfinity
        };
        StatsInternal[stat] = Mathf.Clamp(value, min, max);
    }
    
    public void DealAttackDamageTo(Unit target, Ability ability)
    {
        target.TakeDamage(Stats[Stat.AttackDamage], this, ability);
    }
}