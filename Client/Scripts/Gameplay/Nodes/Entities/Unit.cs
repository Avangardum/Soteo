using System.Collections.Immutable;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public class Unit : KinematicBody2D, IEntity
{
    private Node2D _visuals = null!;
    private Line2D _azimuthLine = null!;
    
    private IServiceProvider _serviceProvider = null!;
    private IEntityManager _entityManager = null!;
    
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
        [Stat.AttackRange] = 100
    }.ToImmutableDictionary(); 
    
    private Queue<ICommand> Commands { get; } = [];
    
    private Dictionary<Stat, float> StatsInternal { get; } = [];
    public IReadOnlyDictionary<Stat, float> Stats => StatsInternal;
    
    protected Dictionary<AbilitySlot, AbilityState> AbilityStatesInternal { get; } = [];
    public ICovariantReadOnlyDictionary<AbilitySlot, IReadOnlyAbilityState> AbilityStates =>
        AbilityStatesInternal.AsCovariant();

    public AbilitySlot? CurrentAbilitySlot { get; private set; }
    public float CurrentAbilityRemainingUseTime { get; private set; } = -1;
    
    public Guid Id { get; set; }

    public Faction Faction { get; set; }
    
    /// <summary>
    /// Visual position of the unit, separate from its physical position. Unlike physical position, this is safe to
    /// set outside _PhysicsProcess, when updating physical position would introduce physics bugs.
    /// </summary>
    public Vector2 VisualPosition
    {
        get => Position + _visuals.Position;
        set => _visuals.Position = value - Position;
    }
    
    public float Azimuth
    {
        get;
        set
        {
            field = Mathf.PosMod(value, 360);
            _azimuthLine.RotationDegrees = field;
        }
    }
    
    Node2D IEntity.Node => this;

    public EntitySnapshot CreateSnapshot()
    {
        return new EntitySnapshot
        {
            Id = Id,
            Position = Position,
            Azimuth = Azimuth,
            Stats = Stats.ToImmutableDictionary(),
            AbilityStates = AbilityStatesInternal
                .ToImmutableDictionary(it => it.Key, IReadOnlyAbilityState (it) => it.Value with {}),
            CurrentAbilitySlot = CurrentAbilitySlot,
            CurrentAbilityRemainingUseTime = CurrentAbilityRemainingUseTime
        };
    }

    public void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        if (snapshot.Position != null) VisualPosition = snapshot.Position.Value;
        if (snapshot.Azimuth != null) Azimuth = snapshot.Azimuth.Value;
        foreach ((Stat stat, float value) in snapshot.Stats) StatsInternal[stat] = value;
        foreach ((AbilitySlot slot, IReadOnlyAbilityState state) in snapshot.AbilityStates)
            AbilityStatesInternal[slot] = new AbilityState(state);
        if (snapshot.CurrentAbilitySlot != null) CurrentAbilitySlot = snapshot.CurrentAbilitySlot.Value;
        if (snapshot.CurrentAbilityRemainingUseTime == -1) CurrentAbilitySlot = null;
        if (snapshot.CurrentAbilityRemainingUseTime != null)
            CurrentAbilityRemainingUseTime = snapshot.CurrentAbilityRemainingUseTime.Value;
    }
    
    private void MatchPhysicsPositionToVisualPosition()
    {
        Position += _visuals.Position;
        _visuals.Position = Vector2.Zero;
    }
    
    [Inject]
    public void Inject(IServiceProvider serviceProvider, IEntityManager entityManager)
    {
        _serviceProvider = serviceProvider;
        _entityManager = entityManager;
    }
    
    public override void _Ready()
    {
        _visuals = GetNode<Node2D>("Visuals");
        _azimuthLine = GetNode<Line2D>("Visuals/AzimuthLine");
        
        foreach (Stat stat in Enum.GetValues<Stat>()) StatsInternal[stat] = DefaultStats[stat];
        
        Faction = Id.GetHashCode() % 2 == 0 ? Faction.Empire : Faction.Syndicate;
    }

    public override void _PhysicsProcess(float deltaTime)
    {
        if (IsServer)
        {
            foreach (AbilityState abilityState in AbilityStatesInternal.Values)
                abilityState.Cooldown = Mathf.Max(abilityState.Cooldown - deltaTime, 0);
            ExecuteCommands(deltaTime);
        }
        else
        {
            MatchPhysicsPositionToVisualPosition();
        }
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
                    LookAtPosition(command.Position, ref remainingDeltaTime);
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
        if (remainingDeltaTime == 0) return;
        
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
        if (remainingDeltaTime == 0) return;
        
        Vector2 desiredMovement = position - Position;
        float desiredMovementLength = desiredMovement.Length();
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
    
    private void UseAbility(UseAbilityCommand command, ref float remainingDeltaTime)
    {
        if (!AbilityStatesInternal.TryGetValue(command.Slot, out AbilityState? state) || state.Cooldown > 0)
        {
            if (!command.Repeat) Commands.Dequeue();
            return;
        }
        
        AbilityUseContext context = GetAbilityUseContext(command);
        
        if (state.Ability.Validate(context) != AbilityValidationResult.Ok)
        {
            if (!command.Repeat) Commands.Dequeue();
            return;
        }
        
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
    
    public AbilityUseContext GetAbilityUseContext(UseAbilityCommand command)
    {
        if (!AbilityStates.TryGetValue(command.Slot, out IReadOnlyAbilityState? state))
            throw new ArgumentException($"Unit {Id} doesn't have an ability in slot {command.Slot}");
        return new AbilityUseContext
        {
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
    
    public void DealDamage(float amount, Unit source, Ability ability)
    {
        ChangeStat(Stat.CurrentHealth, -amount);
    }
    
    public void Heal(float amount, Unit source, Ability ability)
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
}