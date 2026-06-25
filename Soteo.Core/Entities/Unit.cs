using System.Collections.Immutable;
using System.Numerics;
using Soteo.Core.Abilities;
using Soteo.Core.Commands;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;
using Soteo.Core.Statuses;
using Soteo.Util;
using Soteo.Util.Interfaces;

namespace Soteo.Core.Entities;

public abstract class Unit : UnitBase<IUnitNode>, ICommandableUnit
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityManager _entityManager;
    
    private long _nextStatusOrdinal;
    
    private readonly Queue<ICommand> _commands = [];

    private Guid? _controllingPlayerId;
    
    public IReadOnlySet<Guid> ControllingPlayerIds
    {
        get
        {
            if (_controllingPlayerId == null)
                return ReadOnlySetWrapper<Guid>.Empty;
            else
                return new ReadOnlySetWrapper<Guid>(new HashSet<Guid> { _controllingPlayerId.Value } );
        }
    }

    private Dictionary<Guid, StatusContext> StatusesInternal { get; set; } = [];
    public IReadOnlyDictionary<Guid, StatusContext> Statuses => StatusesInternal;
    
    public Unit
    (
        Guid id,
        Guid controllingPlayerId,
        IUnitNode node,
        IEntityManager entityManager,
        IServiceProvider serviceProvider
    ) : base(id, node)
    {
        _serviceProvider = serviceProvider;
        _entityManager = entityManager;
        
        _controllingPlayerId = controllingPlayerId;
    }
    
    public override Vector2 Position
    {
        get => base.Position;
        set
        {
            base.Position = value;
            Node?.Position = Position;
        }
    }
    
    public override EntitySnapshot CreateSnapshot()
    {
        return new UnitSnapshot
        {
            Id = Id,
            IsRemoved = IsRemoved,
            Position = Position,
            Azimuth = Azimuth,
            IsDead = IsDead,
            IsMoving = IsMoving,
            Stats = Stats.ToImmutableDictionary(),
            AbilitySlotStates = AbilitySlotStatesInternal.ToImmutableDictionary(),
            AbilityUseProgress = AbilityUseProgress,
            Statuses = Statuses.ToImmutableDictionary(it => it.Key, it => it.Value.ToSnapshot())
        };
    }

    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        base.ReplicateSnapshot(snapshot);
        var s = (UnitSnapshot)snapshot;
        IsDead = s.IsDead;
        IsMoving = s.IsMoving;
        StatsInternal = s.Stats.ToDictionary();
        AbilitySlotStatesInternal = s.AbilitySlotStates.ToDictionary();
        AbilityUseProgress = s.AbilityUseProgress;
        StatusesInternal =
            s.Statuses.ToDictionary(it => it.Key, it => StatusContext.FromSnapshot(it.Value, _serviceProvider));
        _nextStatusOrdinal = Statuses.Count > 0 ? Statuses.Values.Max(it => it.Ordinal) + 1 : 0;
    }

    public void Tick(double delta)
    {
        if (IsRemoved) return;
        
        IsMoving = false;
        ProcessStatuses(delta);
        UpdateStats();
        DecreaseCooldowns(delta);
        ApplyRegen(delta);
        ExecuteCommands(Node.Required, delta); // todo Required throws when status tick kills the unit
    }
    
    private void DecreaseCooldowns(double delta)
    {
        foreach (AbilitySlot slot in AbilitySlotStatesInternal.Keys.ToList())
        {
            AbilitySlotStatesInternal[slot] = AbilitySlotStatesInternal[slot] with
            {
                Cooldown = Math.Max(AbilitySlotStatesInternal[slot].Cooldown - delta, 0)
            };
        }
    }
    
    private void ApplyRegen(double delta)
    {
        ChangeResourceStat(Stat.CurrentHealth, Stats[Stat.HealthRegen] * delta);
        ChangeResourceStat(Stat.CurrentMana, Stats[Stat.ManaRegen] * delta);
    }
    
    private void ProcessStatuses(double delta)
    {
        ImmutableList<StatusContext> contexts = Statuses.Values.ToImmutableList();
        for (int i = 0; i < contexts.Count; i++)
        {
            StatusContext context = contexts[i];
            double limitedDelta = Math.Min(delta, context.RemainingTime);
            StatusTickContext? newTick = ProcessStatusTickCountdown(context, limitedDelta);
            context = context with
            {
                Tick = newTick,
                ElapsedTime = context.ElapsedTime + limitedDelta,
                DisplayElapsedTime = context.DisplayElapsedTime + limitedDelta,
                RemainingTime = context.RemainingTime - limitedDelta
            };
            if (context.RemainingTime > 0)
                StatusesInternal[context.Id] = context;
            else
                RemoveStatus(context.Id);
        }
    }
    
    private StatusTickContext? ProcessStatusTickCountdown(StatusContext context, double delta)
    {
        if (context.Tick == null) return null;
        double countdown = context.Tick.Countdown - delta;
        while (countdown <= 0)
        {
            context.Status.Tick(context, context.Tick.Interval);
            countdown += context.Tick.Interval;
        }
        return context.Tick with { Countdown = countdown };
    }
    
    private void UpdateStats()
    {
        Dictionary<Stat, Dictionary<StatModifierKind, List<StatModifier>>> modifiers = [];
        
        foreach (Stat stat in Stat.AllComputed)
        {
            modifiers[stat] = [];
            foreach (StatModifierKind kind in Enum.GetValues<StatModifierKind>())
            {
                modifiers[stat][kind] = [];
            }
        }
        
        foreach (StatModifier modifier in Statuses.Values.SelectMany(it => it.Status.StatModifiers(it)))
            modifiers[modifier.Stat][modifier.Kind].Add(modifier);
        
        double oldMaxHealth = Stats[Stat.MaxHealth];
        double oldMaxMana = Stats[Stat.MaxMana];
        
        foreach (Stat stat in Stat.AllComputed)
            StatsInternal[stat] = CalculateStatValue(stat, modifiers[stat]);
        
        UpdateResourceStat(Stat.CurrentHealth, Stat.MaxHealth, oldMaxHealth);
        UpdateResourceStat(Stat.CurrentMana, Stat.MaxMana, oldMaxMana);
    }

    private double CalculateStatValue
    (
        Stat stat,
        Dictionary<StatModifierKind, List<StatModifier>> modifiers
    )
    {
        StatModifier? setModifier = modifiers[StatModifierKind.Set]
            .OrderByDescending(it => it.Priority)
            .ThenByDescending(it => it.Value)
            .FirstOrDefault();
        if (setModifier != null) return setModifier.Value;

        List<StatModifier> floorModifiers = modifiers[StatModifierKind.Floor];
        List<StatModifier> ceilingModifiers = modifiers[StatModifierKind.Ceiling];
        double maxFloor = floorModifiers
            .Select(it => it.Value)
            .OrderDescending()
            .FirstOrDefault(StatConst[stat].Min);
        double minCeiling = ceilingModifiers
            .Select(it => it.Value)
            .Order()
            .FirstOrDefault(StatConst[stat].Max);
        if (maxFloor > minCeiling)
            return ResolveNonOverlappingStatLimits(floorModifiers, ceilingModifiers);
            
        double addTotal = modifiers[StatModifierKind.Add].Sum(it => it.Value);
        double multiplyTotal = modifiers[StatModifierKind.Multiply].Product(it => it.Value);
        return Maths.Clamp((StatConst[stat].Default + addTotal) * multiplyTotal, maxFloor, minCeiling);
    }
    
    private double ResolveNonOverlappingStatLimits
    (
        IReadOnlyList<StatModifier> floorModifiers,
        IReadOnlyList<StatModifier> ceilingModifiers
    )
    {
        var floorStack = new Stack<StatModifier>(floorModifiers.OrderBy(it => it.Value));
        var ceilingStack = new Stack<StatModifier>(ceilingModifiers.OrderByDescending(it => it.Value));
        
        StatModifier floor = floorStack.Pop();
        StatModifier ceiling = ceilingStack.Pop();

        while (true)
        {
            if (ceiling.Priority > floor.Priority)
            {
                if (floorStack.Count == 0) return ceiling.Value;
                floor = floorStack.Pop();
            }
            else
            {
                if (ceilingStack.Count == 0) return floor.Value;
                ceiling = ceilingStack.Pop();
            }
        }
    }
    
    private void UpdateResourceStat(Stat stat, Stat maxStat, double oldMax)
    {
        double normalized = Stats[stat] / oldMax;
        SetResourceStat(stat, Stats[maxStat] * normalized);
    }
    
    private void ExecuteCommands(IUnitNode node, double deltaTime)
    {
        double remainingDeltaTime = deltaTime;
        int iterations = 0;
        const int maxIterations = 5;
        while (_commands.Count > 0 && remainingDeltaTime > 0 && iterations < maxIterations)
        {
            iterations++;
            switch (_commands.Peek())
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
    
    private void LookAtPosition(Vector2 position, ref double remainingDeltaTime)
    {
        LookInDirection(position - Position, ref remainingDeltaTime);
    }
    
    private void LookInDirection(Vector2 direction, ref double remainingDeltaTime)
    {
        LookAtAzimuth(Maths.DirectionToAzimuth(direction), ref remainingDeltaTime);
    }
    
    private void LookAtAzimuth(double azimuth, ref double remainingDeltaTime)
    {
        if (remainingDeltaTime == 0 || Stats[Stat.TurnSpeed] == 0) return;
        
        double desiredDeltaAzimuth = Maths.ModularDelta(Azimuth, azimuth, 360);
        
        double timeToComplete = Math.Abs(desiredDeltaAzimuth) / Stats[Stat.TurnSpeed];
        if (remainingDeltaTime < timeToComplete)
        {
            Azimuth += Maths.Sign(desiredDeltaAzimuth) * remainingDeltaTime * Stats[Stat.TurnSpeed];
            remainingDeltaTime = 0;
        }
        else
        {
            Azimuth = azimuth;
            remainingDeltaTime -= timeToComplete;
            if (_commands.PeekOrDefault() is LookCommand)
                _commands.Dequeue();
        }
    }
    
    private void MoveToPosition(Vector2 position, ref double remainingDeltaTime, IUnitNode node)
    {
        LookAtPosition(position, ref remainingDeltaTime);
        if (remainingDeltaTime == 0 || Stats[Stat.MoveSpeed] == 0) return;
        
        Vector2 desiredMovement = position - Position;
        double desiredMovementLength = desiredMovement.Length();
        if (desiredMovementLength == 0)
        {
            if (_commands.PeekOrDefault() is MoveCommand) _commands.Dequeue();
            return;
        }
        Vector2 normalizedDesiredMovement = desiredMovement / desiredMovementLength;
        double timeToComplete = desiredMovementLength / Stats[Stat.MoveSpeed];
        if (remainingDeltaTime < timeToComplete)
        {
            Vector2 movement = normalizedDesiredMovement * Stats[Stat.MoveSpeed] * remainingDeltaTime;
            MoveAndCollide(movement, node);
            remainingDeltaTime = 0;
        }
        else
        {
            MoveAndCollide(desiredMovement, node);
            remainingDeltaTime -= timeToComplete;
            if (_commands.PeekOrDefault() is MoveCommand) _commands.Dequeue();
        }
        IsMoving = true;
    }
    
    private void MoveAndCollide(Vector2 movement, IUnitNode node)
    {
        node.MoveAndCollide(movement);
        Position = node.Position;
    }

    private void UseAbility(UseAbilityCommand command, ref double remainingDeltaTime, IUnitNode node)
    {
        AbilityContext? context = GetAbilityContext(command);
        if (context == null)
        {
            _commands.Dequeue();
            AbilityUseProgress = null;
            return;
        }
        
        if (AbilitySlotStates[command.Slot].Cooldown > 0)
        {
            if (command.Repeat)
                WaitForAbilityCooldown(context, ref remainingDeltaTime);
            else
                _commands.Dequeue();
            AbilityUseProgress = null;
            return;
        }
        
        AbilityValidationResult validationResult =
            ValidateAbilityWithCorrection(context.Ability, context, command, ref remainingDeltaTime, node);
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
                RemainingTime = context.Ability.UseTime(context)
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
    
    private AbilityContext? GetAbilityContext(UseAbilityCommand command)
    {
        if (!AbilitySlotStates.TryGetValue(command.Slot, out AbilitySlotState? state)) return null;
        IUnit? targetUnit = null;
        if (command.TargetUnitId != null)
        {
            targetUnit = _entityManager.GetEntity(command.TargetUnitId.Value) as Unit;
            if (targetUnit == null) return null;
        }
        return new AbilityContext
        {
            Ability = state.Ability,
            Level = state.Level,
            User = this,
            UserStats = Stats.ToImmutableDictionary(),
            ServiceProvider = _serviceProvider,
            TargetPosition = command.TargetPosition,
            TargetUnit = targetUnit,
            TargetDirection = command.TargetDirection,
            TargetShardId = command.TargetShardId
        };
    }
    
    private void TriggerAbilityEffect(AbilityContext context, UseAbilityCommand command)
    {
        context.Ability.TakeEffect(context);
        double cooldown = context.Ability.Cooldown(context);
        AbilitySlotStatesInternal[command.Slot] = AbilitySlotStatesInternal[command.Slot] with
        {
            Cooldown = cooldown,
            MaxCooldown = cooldown
        };
        AbilityUseProgress = null;
        if (!command.Repeat)
            _commands.Dequeue();
    }
    
    private void WaitForAbilityCooldown(AbilityContext context, ref double remainingDeltaTime)
    {
        Vector2? targetPosition = context.TargetPosition ?? context.TargetUnit?.Position;
        if (targetPosition != null)
            LookAtPosition(targetPosition.Value, ref remainingDeltaTime);
        remainingDeltaTime = 0;
    }
    
    /// <summary>
    /// Validate an ability and, if validation fails, try to make it pass
    /// </summary>
    private AbilityValidationResult ValidateAbilityWithCorrection
    (
        Ability ability,
        AbilityContext context,
        UseAbilityCommand command,
        ref double remainingDeltaTime,
        IUnitNode node
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
                    if (command.Repeat)
                        remainingDeltaTime = 0;
                    else
                        _commands.Dequeue();
                    return abilityValidationResult;
            }
        } while (remainingDeltaTime > 0 && iterations < maxIterations);
        return abilityValidationResult;
    }
    
    public void SetCommand(ICommand command)
    {
        _commands.Clear();
        if (command is not StopCommand)
            _commands.Enqueue(command);
        if (command is not UseAbilityCommand useAbilityCommand || useAbilityCommand.Slot != AbilityUseProgress?.Slot)
            AbilityUseProgress = null;
    }
    
    public void SpendHealth(double amount, Ability? sourceAbility)
    {
        ChangeResourceStat(Stat.CurrentHealth, -amount);
    }
    
    public void SpendMana(double amount, Ability? sourceAbility)
    {
        ChangeResourceStat(Stat.CurrentMana, -amount);
    }
    
    public void TakeDamage(double amount, IUnit? sourceUnit, Ability? sourceAbility) =>
        ChangeResourceStat(Stat.CurrentHealth, -amount);

    public void RestoreHealth(double amount, IUnit? sourceUnit, Ability? sourceAbility) =>
        ChangeResourceStat(Stat.CurrentHealth, amount);

    public void RestoreMana(double amount, IUnit? sourceUnit, Ability? sourceAbility) =>
        ChangeResourceStat(Stat.CurrentMana, amount);
    
    protected void ChangeResourceStat(Stat stat, double delta) =>
        SetResourceStat(stat, Stats[stat] + delta);

    protected void SetResourceStat(Stat stat, double value)
    {
        if (!stat.IsResource)
            throw new ArgumentException($"{nameof(SetResourceStat)} can only be used with resource stats");
        if (IsRemoved) return;
        
        double min = 0;
        double max = stat switch
        {
            Stat.CurrentHealth => Stats[Stat.MaxHealth],
            Stat.CurrentMana => Stats[Stat.MaxMana],
            _ => double.PositiveInfinity
        };
        StatsInternal[stat] = Maths.Clamp(value, min, max);
        
        if (stat == Stat.CurrentHealth && Stats[Stat.CurrentHealth] == 0)
            Die();
    }
    
    public void DealAttackDamageTo(IUnit target, Ability sourceAbility)
    {
        target.TakeDamage(Stats[Stat.AttackDamage], this, sourceAbility);
        foreach (StatusContext statusContext in Statuses.Values.ToList())
            statusContext.Status.OnDealAttackDamage(statusContext, target, Stats[Stat.AttackDamage]);
    }
    
    protected void SetAbility<T>(AbilitySlot slot, int level) where T : Ability, new() =>
        SetAbility(Ability.Instance<T>(), slot, level);
    
    protected void SetAbility(Ability ability, AbilitySlot slot, int level)
    {
        if (AbilitySlotStates.ContainsKey(slot))
            throw new InvalidOperationException($"Slot {slot} already has an ability");
        
        AbilitySlotStatesInternal[slot] = new AbilitySlotState { Ability = ability, Level = level };
        if (ability.PassiveStatus != null)
        {
            AbilityContext abilityContext = GetAbilityContext(new UseAbilityCommand(slot)).Required;
            AddStatus
            (
                ability.PassiveStatus,
                double.PositiveInfinity,
                ability.PassiveTickInterval,
                sourceUnit: this,
                sourceAbilityContext: abilityContext
            );
        }
    }
    
    public void AddStatus
    (
        Status status,
        double time,
        double? tickInterval,
        IUnit? sourceUnit,
        AbilityContext? sourceAbilityContext
    )
    {
        if (time < 0) throw new ArgumentException();
        const double minTickInterval = Const.TickInterval;
        if (tickInterval < minTickInterval) throw new ArgumentException();
        if (IsRemoved) return;
        
        StatusContext context = new StatusContext
        {
            Id = Guid.NewGuid(),
            Status = status,
            SourceAbilityContext = sourceAbilityContext,
            Unit = this,
            SourceUnit = sourceUnit,
            Tick = tickInterval == null ? null : new StatusTickContext
            {
                Interval = tickInterval.Value,
                Countdown = tickInterval.Value,
            },
            ElapsedTime = 0,
            DisplayElapsedTime = 0,
            RemainingTime = time,
            Ordinal = _nextStatusOrdinal++,
            ServiceProvider = _serviceProvider
        };
        
        List<StatusContext> duplicates = Statuses.Values.Where(it => it.Status == status).ToList();
        if (duplicates.Count == 0)
        {
            StatusesInternal[context.Id] = context;
        }
        else
        {
            switch (status.DuplicateResolution)
            {
                case DuplicateStatusResolution.Stack:
                    StatusesInternal[context.Id] = context;
                    break;
                case DuplicateStatusResolution.Refresh:
                    RefreshDuplicateStatuses(context, duplicates.Single());
                    break;
                case DuplicateStatusResolution.StackAndRefresh:
                    StatusesInternal[context.Id] = context;
                    RefreshDuplicateStatuses(context, duplicates);
                    break;
                case DuplicateStatusResolution.Replace:
                    RemoveStatus(duplicates.Single().Id);
                    StatusesInternal[context.Id] = context;
                    break;
                case DuplicateStatusResolution.Discard:
                    break;
                case DuplicateStatusResolution.Throw:
                    throw new InvalidOperationException($"{status} does not allow duplicates");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        UpdateStats();
    }
    
    private void RefreshDuplicateStatuses(StatusContext reference, params IReadOnlyList<StatusContext> targets)
    {
        foreach (StatusContext target in targets)
        {
            StatusesInternal[target.Id] = target with
            {
                SourceUnit = reference.SourceUnit,
                SourceAbilityContext = reference.SourceAbilityContext,
                DisplayElapsedTime = 0,
                RemainingTime = Math.Max(target.RemainingTime, reference.RemainingTime)
            };
        }
    }
    
    public void RemoveStatus(Guid id)
    {
        if (IsRemoved) return;
        StatusesInternal.Remove(id);
        UpdateStats();
    }
    
    public void Die()
    {
        if (IsRemoved) return;
        IsDead = true;
        Remove();
    }

    public bool IsAlliedTo(IUnit other) => Faction != Faction.Neutral && other.Faction == Faction;
}
