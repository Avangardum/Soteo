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

public abstract class Unit : Entity<Unit.UnitNode>
{
    public sealed class UnitNode(Unit unit) : KinematicBody2D
    {
        public Unit Unit => unit;
        
        public override void _PhysicsProcess(float delta)
        {
            if (IsServer) unit._PhysicsProcessServer(delta);
            else unit._PhysicsProcessClient(delta);
        }
    }
    
    private readonly Node2D _visuals;
    private readonly Line2D _azimuthLine;
    private readonly EntityProperties _properties;
    
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityManager _entityManager;
    
    protected Unit(Guid id, PackedScene scene, IServiceProvider serviceProvider) :
        base(id, serviceProvider.GetRequiredService<ClientDependency<ICamera>>())
    {
        _serviceProvider = serviceProvider;
        
        _entityManager = serviceProvider.GetRequiredService<IEntityManager>();
        
        Node = new UnitNode(this) { Name = $"{GetType().Name} {id}"};
        scene.InstanceAndReparentTo(Node);
        serviceProvider.GetRequiredService<IShard>().EntityRoot.AddChild(Node);
        
        _visuals = Node.GetNode<Node2D>("Visuals");
        _azimuthLine = Node.GetNode<Line2D>("Visuals/AzimuthLine");
        _properties = Node.GetNode<EntityProperties>("Properties");
        
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
    
    private Dictionary<Stat, float> StatsInternal { get; } = [];
    public IReadOnlyDictionary<Stat, float> Stats => StatsInternal;
    
    protected Dictionary<AbilitySlot, AbilityState> AbilityStatesInternal { get; } = [];
    public ICovariantReadOnlyDictionary<AbilitySlot, IReadOnlyAbilityState> AbilityStates =>
        AbilityStatesInternal.AsCovariant();

    public AbilitySlot? CurrentAbilitySlot { get; private set; }
    public float CurrentAbilityRemainingUseTime { get; private set; } = -1;
    protected override UnitNode? Node => field.AsValid();
    public Faction Faction { get; set; }

    public override Vector2 Position
    {
        get;
        set
        {
            field = value;
            if (IsServer) Node.Position = Position;
            else _visuals.Position = RoundVisualPositionToPixelPerfect(Position, Camera.Value,
                _properties.HalfPixelXVisualOffset, _properties.HalfPixelYVisualOffset) - Node.Position;
        }
    }
    
    public override float Azimuth
    {
        get => base.Azimuth;
        set
        {
            base.Azimuth = value;
            _azimuthLine.RotationDegrees = value;
        }
    }
    
    public override EntitySnapshot CreateSnapshot()
    {
        return new UnitSnapshot(Id)
        {
            Position = Position,
            Azimuth = Azimuth,
            Stats = Stats.ToImmutableDictionary(),
            AbilityStates = AbilityStatesInternal
                .ToImmutableDictionary(it => it.Key, IReadOnlyAbilityState (it) => it.Value with {}),
            CurrentAbilitySlot = CurrentAbilitySlot,
            CurrentAbilityRemainingUseTime = CurrentAbilityRemainingUseTime
        };
    }

    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        var s = (UnitSnapshot)snapshot;
        
        if (s.Position != null) Position = s.Position.Value;
        if (s.Azimuth != null) Azimuth = s.Azimuth.Value;
        foreach ((Stat stat, float value) in s.Stats) StatsInternal[stat] = value;
        foreach ((AbilitySlot slot, IReadOnlyAbilityState state) in s.AbilityStates)
            AbilityStatesInternal[slot] = new AbilityState(state);
        if (s.CurrentAbilitySlot != null) CurrentAbilitySlot = s.CurrentAbilitySlot.Value;
        if (s.CurrentAbilityRemainingUseTime == -1) CurrentAbilitySlot = null;
        if (s.CurrentAbilityRemainingUseTime != null)
            CurrentAbilityRemainingUseTime = s.CurrentAbilityRemainingUseTime.Value;
    }

    protected override void OnZoomChanged()
    {
        // Trigger Position setter to recalculate position of visuals
        Position = Position;
    }

    public virtual void _PhysicsProcessServer(float deltaTime)
    {
        foreach (AbilityState abilityState in AbilityStatesInternal.Values)
            abilityState.Cooldown = Mathf.Max(abilityState.Cooldown - deltaTime, 0);
        ExecuteCommands(deltaTime);
    }

    public virtual void _PhysicsProcessClient(float deltaTime)
    {
        Node.Position = Position;
        _visuals.Position = Vector2.Zero;
    }

    private void ExecuteCommands(float deltaTime)
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
                    MoveToPosition(command.Position, ref remainingDeltaTime);
                    break;
                case UseAbilityCommand command:
                    UseAbility(command, ref remainingDeltaTime);
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
        
        float desiredDeltaAzimuth = azimuth - Azimuth;
        if (desiredDeltaAzimuth > 180) desiredDeltaAzimuth -= 360;
        if (desiredDeltaAzimuth < -180) desiredDeltaAzimuth += 360;
        
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
    
    private void MoveToPosition(Vector2 position, ref float remainingDeltaTime)
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
        float timeToComplete = desiredMovementLength / Stats[Stat.MoveSpeed];
        if (timeToComplete <= remainingDeltaTime)
        {
            MoveAndCollide(desiredMovement);
            remainingDeltaTime -= timeToComplete;
            if (Commands.PeekOrDefault() is MoveCommand) Commands.Dequeue();
        }
        else
        {
            MoveAndCollide(desiredMovement / desiredMovementLength * Stats[Stat.MoveSpeed] * remainingDeltaTime);
            remainingDeltaTime = 0;
        }
    }
    
    private KinematicCollision2D MoveAndCollide(Vector2 movement)
    {
        KinematicCollision2D collision = Node.MoveAndCollide(movement);
        Position = Node.Position;
        return collision;
    }

    private void UseAbility(UseAbilityCommand command, ref float remainingDeltaTime)
    {
        if (!AbilityStatesInternal.TryGetValue(command.Slot, out AbilityState? state))
        {
            Commands.Dequeue();
            return;
        }
        
        AbilityContext context = GetAbilityContext(command);
        
        if (state.Cooldown > 0)
        {
            if (command.Repeat)
            {
                Vector2? targetPosition = context.TargetPosition ?? context.TargetUnit?.Position;
                if (targetPosition != null) LookAtPosition(targetPosition.Value, ref remainingDeltaTime);
                remainingDeltaTime = 0;
            }
            else Commands.Dequeue();
            return;
        }
        
        AbilityValidationResult validationResult =
            ValidateAbilityWithCorrection(state.Ability, context, command, ref remainingDeltaTime);
        if (validationResult != AbilityValidationResult.Ok || remainingDeltaTime == 0) return;
        
        if (command.Slot != CurrentAbilitySlot) CurrentAbilityRemainingUseTime = state.Ability.UseTime(context);
        CurrentAbilitySlot = command.Slot;
        
        if (remainingDeltaTime < CurrentAbilityRemainingUseTime)
        {
            CurrentAbilityRemainingUseTime -= remainingDeltaTime;
            remainingDeltaTime = 0;
        }
        else
        {
            remainingDeltaTime -= CurrentAbilityRemainingUseTime;
            state.Ability.TakeEffect(context);
            state.Cooldown = state.Ability.Cooldown(context);
            CurrentAbilitySlot = null;
            CurrentAbilityRemainingUseTime = -1;
            if (!command.Repeat) Commands.Dequeue();
        }
    }
    
    public AbilityContext GetAbilityContext(UseAbilityCommand command)
    {
        if (!AbilityStates.TryGetValue(command.Slot, out IReadOnlyAbilityState? state))
            throw new ArgumentException($"Unit {Id} doesn't have an ability in slot {command.Slot}");
        return new AbilityContext
        {
            Ability = state.Ability,
            Level = state.Level,
            User = this,
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
        ref float remainingDeltaTime
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
                    MoveToPosition(targetPosition!.Value, ref remainingDeltaTime);
                    break;
                case AbilityValidationResult.OutOfAngularRange:
                    LookAtAzimuth(SoteoMath.DirectionToAzimuth(targetPosition!.Value - Position),
                        ref remainingDeltaTime);
                    break;
                default:
                    if (command.Repeat) remainingDeltaTime = 0;
                    else Commands.Dequeue();
                    return abilityValidationResult;
            }
        } while (remainingDeltaTime > 0 && iterations < maxIterations);
        return abilityValidationResult;
    }

    public void SetCommand(ICommand command)
    {
        Commands.Clear();
        Commands.Enqueue(command);
        if (command is not UseAbilityCommand useAbilityCommand || useAbilityCommand.Slot != CurrentAbilitySlot)
        {
            CurrentAbilitySlot = null;
            CurrentAbilityRemainingUseTime = -1;
        }
    }
     
    public void CancelCommands()
    {
        Commands.Clear();
    }
    
    public bool IsAlliedTo(Unit other) => other.Faction == Faction;
    
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
    
    public static Unit? FromNode(Node node) => (node as UnitNode)?.Unit;
}