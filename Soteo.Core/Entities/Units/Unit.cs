using Soteo.Core.Commands;

namespace Soteo.Core.Entities.Units;

public abstract class Unit : UnitBase<IUnitNode>
{
    private readonly Services _services;
    private readonly IEntityManager _entityManager;
    private readonly ICrossInventoryHelper _crossInventoryHelper;

    private long _nextStatusOrdinal;
    private readonly Queue<Command> _commands = [];
    private Guid? _controllingPlayerId;
    private Dictionary<Guid, StatusContext> _statusContexts = [];
    private Dictionary<Stat, double> _stats  = [];
    private Dictionary<AbilitySlot, AbilitySlotState> _abilitySlotStates = [];
    private List<Ability> _innateHotbarAbilities = [];
    private Dictionary<SpecialAbilitySlot, Ability> _innateSpecialAbilities = [];
    private bool _allowAbilityPassiveStatusManipulation;

    public Inventory Inventory { get; }
    public IEquipmentInventory EquipmentInventory { get; }

    protected Unit
    (
        Guid id,
        Guid controllingPlayerId,
        IUnitNode node,
        IEntityManager entityManager,
        Inventory inventory,
        IEquipmentInventory equipmentInventory,
        ICrossInventoryHelper crossInventoryHelper,
        Services services
    ) : base(id, node)
    {
        _services = services;
        _entityManager = entityManager;
        _controllingPlayerId = controllingPlayerId;
        Inventory = inventory;
        EquipmentInventory = equipmentInventory;
        _crossInventoryHelper = crossInventoryHelper;

        foreach (Stat stat in Stat.All)
            _stats[stat] = StatConst[stat].Default;

        EquipmentInventory.Changed += () => { UpdateHotbarAbilityList(); UpdateSpecialAbilityList(); };
    }

    // For testing
    private protected IReadOnlyList<Command> GetCommands() => _commands.ToList();

    public IReadOnlyList<Guid> ControllingPlayerIds =>
        _controllingPlayerId == null ? [] : [_controllingPlayerId.Value];

    public IReadOnlyDictionary<Guid, StatusContext> GetStatusContexts() =>
        _statusContexts.ToDictionary().AsReadOnly();

    public IReadOnlyDictionary<Stat, double> GetStats() =>
        _stats.ToDictionary().AsReadOnly();

    public IReadOnlyDictionary<AbilitySlot, AbilitySlotState> GetAbilitySlotStates() =>
        _abilitySlotStates.ToDictionary().AsReadOnly();

    protected IReadOnlyList<Ability> GetInnateHotbarAbilities() =>
        _innateHotbarAbilities.ToList();

    protected IReadOnlyDictionary<SpecialAbilitySlot, Ability> GetInnateSpecialAbilities() =>
        _innateSpecialAbilities.ToDictionary();

    public override Vector2 Position
    {
        get => base.Position;
        set
        {
            base.Position = value;
            Node?.PositionMeters = Position;
        }
    }

    public override EntitySnapshot ToSnapshot()
    {
        return new UnitSnapshot
        {
            Id = Id,
            IsRemoved = IsRemoved,
            Position = Position,
            Azimuth = Azimuth,
            IsDead = IsDead,
            IsMoving = IsMoving,
            Stats = GetStats(),
            AbilitySlotStates = _abilitySlotStates.ToDictionary().AsReadOnly(),
            AbilityUseProgress = AbilityUseProgress,
            StatusesContexts = _statusContexts.ToDictionary(it => it.Key, it => it.Value.ToSnapshot()).AsReadOnly(),
            ControllingPlayerId = _controllingPlayerId,
            InventorySnapshot = Inventory.ToSnapshot(),
            EquipmentInventory = EquipmentInventory.ToSnapshot(),
            InnateHotbarAbilities = _innateHotbarAbilities.ToList().AsReadOnly(),
            InnateSpecialAbilities = _innateSpecialAbilities.ToDictionary().AsReadOnly()
        };
    }

    public override void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        base.ReplicateSnapshot(snapshot);
        var s = (UnitSnapshot)snapshot;
        IsDead = s.IsDead;
        IsMoving = s.IsMoving;
        _stats = s.Stats.ToDictionary();
        _abilitySlotStates = s.AbilitySlotStates.ToDictionary();
        AbilityUseProgress = s.AbilityUseProgress;
        _statusContexts =
            s.StatusesContexts.ToDictionary(it => it.Key, it => StatusContext.FromSnapshot(it.Value, _services));
        _nextStatusOrdinal = _statusContexts.Count > 0 ? _statusContexts.Values.Max(it => it.Ordinal) + 1 : 0;
        _controllingPlayerId = s.ControllingPlayerId;
        Inventory.ReplicateSnapshot(s.InventorySnapshot);
        EquipmentInventory.ReplicateSnapshot(s.EquipmentInventory);
        _innateHotbarAbilities = s.InnateHotbarAbilities.ToList();
        _innateSpecialAbilities = s.InnateSpecialAbilities.ToDictionary();

        UpdateHotbarAbilityList();
        UpdateSpecialAbilityList();
    }

    public override void Tick(double delta)
    {
        if (IsRemoved) return;
        IsMoving = false;
        // todo check IsRemoved between ticking statuses
        ProcessStatuses(delta);
        if (IsRemoved) return;
        UpdateStats();
        if (IsRemoved) return;
        DecreaseCooldowns(delta);
        if (IsRemoved) return;
        ApplyRegen(delta);
        if (IsRemoved) return;
        // todo check IsRemoved between executing commands
        ExecuteCommands(Node.Required, delta);
    }

    private void DecreaseCooldowns(double delta)
    {
        foreach (AbilitySlot slot in _abilitySlotStates.Keys.ToList())
        {
            _abilitySlotStates[slot] = _abilitySlotStates[slot] with
            {
                Cooldown = Math.Max(_abilitySlotStates[slot].Cooldown - delta, 0)
            };
        }
    }

    private void ApplyRegen(double delta)
    {
        ChangeResourceStat(Stat.CurrentHealth, _stats[Stat.HealthRegen] * delta);
        ChangeResourceStat(Stat.CurrentMana, _stats[Stat.ManaRegen] * delta);
    }

    private void ProcessStatuses(double delta)
    {
        // todo if a status tick removes another status, that status will still be ticked
        foreach (StatusContext context in _statusContexts.Values.ToList())
        {
            // todo check if self-removing status behaves correctly
            context.Status.Tick(context, delta);
            if (context.RemainingTime > delta)
            {
                _statusContexts[context.Id] = context with
                {
                    ElapsedTime = context.ElapsedTime + delta,
                    DisplayElapsedTime = context.DisplayElapsedTime + delta,
                    RemainingTime = context.RemainingTime - delta
                };
            }
            else
            {
                RemoveStatus(context.Id);
            }
        }
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

        foreach (StatModifier modifier in _statusContexts.Values.SelectMany(it => it.Status.StatModifiers(it)))
            modifiers[modifier.Stat][modifier.Kind].Add(modifier);

        double oldMaxHealth = _stats[Stat.MaxHealth];
        double oldMaxMana = _stats[Stat.MaxMana];

        foreach (Stat stat in Stat.AllComputed)
            _stats[stat] = CalculateStatValue(stat, modifiers[stat]);

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
        // todo will crash if a floor is above max or ceiling is below min

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
        double normalized = _stats[stat] / oldMax; // todo division by 0
        SetResourceStat(stat, _stats[maxStat] * normalized);
    }

    private void ExecuteCommands(IUnitNode node, double deltaTime)
    {
        // todo Bad stuff will happen if the command stack changes during execution.
        // While this shouldn't happen, some guard against this should probably be added.
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
                case MoveItemStackCommand command:
                    MoveItemStack(command);
                    break;
                case EquipCommand command:
                    Equip(command);
                    break;
                case UnequipCommand command:
                    Unequip(command);
                    break;
                case Command command:
                    throw new Exception($"Unknown command type {command.GetType()}");
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
        if (remainingDeltaTime == 0 || _stats[Stat.TurnSpeed] == 0) return;

        double desiredDeltaAzimuth = Maths.ModularDelta(Azimuth, azimuth, 360);

        double timeToComplete = Math.Abs(desiredDeltaAzimuth) / _stats[Stat.TurnSpeed];
        if (remainingDeltaTime < timeToComplete)
        {
            Azimuth += Maths.Sign(desiredDeltaAzimuth) * remainingDeltaTime * _stats[Stat.TurnSpeed];
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
        if (remainingDeltaTime == 0 || _stats[Stat.MoveSpeed] == 0) return;

        Vector2 desiredMovement = position - Position;
        double desiredMovementLength = desiredMovement.Length();
        if (desiredMovementLength == 0)
        {
            if (_commands.PeekOrDefault() is MoveCommand) _commands.Dequeue();
            return;
        }
        Vector2 normalizedDesiredMovement = desiredMovement / desiredMovementLength;
        double timeToComplete = desiredMovementLength / _stats[Stat.MoveSpeed];
        if (remainingDeltaTime < timeToComplete)
        {
            Vector2 movement = normalizedDesiredMovement * _stats[Stat.MoveSpeed] * remainingDeltaTime;
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
        Position = node.PositionMeters;
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

        if (_abilitySlotStates[command.Slot].Cooldown > 0)
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
        if (!_abilitySlotStates.TryGetValue(command.Slot, out AbilitySlotState? state)) return null;
        Unit? targetUnit = null;
        if (command.TargetUnitId != null)
        {
            targetUnit = _entityManager.GetEntity(command.TargetUnitId.Value) as Unit;
            if (targetUnit == null) return null;
        }
        return new AbilityContext
        {
            Ability = state.Ability,
            User = this,
            Alt = command.Alt,
            UserStats = GetStats(),
            Services = _services,
            TargetPosition = command.TargetPosition,
            TargetUnit = targetUnit,
            TargetDirection = command.TargetDirection,
            TargetShardId = command.TargetShardId
        };
    }

    private AbilityContext GetPassiveAbilityContext(Ability ability)
    {
        return new AbilityContext
        {
            Ability = ability,
            User = this,
            Alt = false,
            UserStats = GetStats(),
            Services = _services,
            TargetPosition = null,
            TargetUnit = null,
            TargetDirection = null,
            TargetShardId = null
        };
    }

    private void TriggerAbilityEffect(AbilityContext context, UseAbilityCommand command)
    {
        context.Ability.TakeEffect(context);
        double cooldown = context.Ability.Cooldown(context);
        _abilitySlotStates[command.Slot] = _abilitySlotStates[command.Slot] with
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
            AbilityValidationMode mode =
                AbilityUseProgress == null ? AbilityValidationMode.Initial : AbilityValidationMode.Continuous;
            abilityValidationResult = ability.Validate(context, mode);
            switch (abilityValidationResult)
            {
                case AbilityValidationResult.Ok:
                    return abilityValidationResult;
                case AbilityValidationResult.OutOfRange:
                    // todo handle targetPosition == null
                    MoveToPosition(targetPosition!.Value, ref remainingDeltaTime, node);
                    break;
                case AbilityValidationResult.OutOfAngularRange:
                    // todo handle targetPosition == null
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

    private void MoveItemStack(MoveItemStackCommand command)
    {
        _commands.Dequeue();

        if (command.FromIndex < 0) return;
        if (command.FromIndex >= Inventory.Count) return;
        if (command.ToIndex < 0) return;
        if (command.ToIndex >= Inventory.Count) return;

        ItemStack? movedStack = Inventory.Remove(command.FromIndex);
        if (movedStack == null) return;
        ItemStack? returnedStack = Inventory.Put(command.ToIndex, movedStack);
        if (returnedStack != null)
            Inventory.Put(command.FromIndex, returnedStack);
    }

    private void Equip(EquipCommand command)
    {
        _commands.Dequeue();
        _crossInventoryHelper.Equip(Inventory, EquipmentInventory, command.InventorySlot, toEquipmentSlot: null);
    }

    private void Unequip(UnequipCommand command)
    {
        _commands.Dequeue();
        _crossInventoryHelper.Unequip(Inventory, EquipmentInventory, command.EquipmentSlot, command.ToInventorySlot);
    }

    internal void SetCommand(Command command)
    {
        Command? prevCommand = _commands.PeekOrDefault();
        _commands.Clear();

        if (command is UseAbilityCommand a && prevCommand is UseAbilityCommand b && a.Slot == b.Slot)
        {
            _commands.Enqueue(prevCommand);
            return;
        }

        if (command is not StopCommand)
            _commands.Enqueue(command);

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

    public void TakeDamage(Damage damage, Unit? sourceUnit, Ability? sourceAbility) =>
        ChangeResourceStat(Stat.CurrentHealth, -damage.Amount);

    public void TakeDamage(Damage damage, ISourceUnitAndAbility? source) =>
        TakeDamage(damage, source?.Unit, source?.Ability);

    public void RestoreHealth(double amount, Unit? sourceUnit, Ability? sourceAbility) =>
        ChangeResourceStat(Stat.CurrentHealth, amount);

    public void RestoreHealth(double amount, ISourceUnitAndAbility? source) =>
        RestoreHealth(amount, source?.Unit, source?.Ability);

    public void RestoreMana(double amount, Unit? sourceUnit, Ability? sourceAbility) =>
        ChangeResourceStat(Stat.CurrentMana, amount);

    public void RestoreMana(double amount, ISourceUnitAndAbility? source) =>
        RestoreMana(amount, source?.Unit, source?.Ability);

    protected void ChangeResourceStat(Stat stat, double delta) =>
        SetResourceStat(stat, _stats[stat] + delta);

    protected void SetResourceStat(Stat stat, double value)
    {
        if (!stat.IsResource)
            throw new ArgumentException($"{nameof(SetResourceStat)} can only be used with resource stats");
        if (IsRemoved) return;

        double min = 0;
        double max = stat switch
        {
            Stat.CurrentHealth => _stats[Stat.MaxHealth],
            Stat.CurrentMana => _stats[Stat.MaxMana],
            _ => double.PositiveInfinity
        };
        _stats[stat] = Maths.Clamp(value, min, max);

        if (stat == Stat.CurrentHealth && _stats[Stat.CurrentHealth] == 0)
            Die();
    }

    public void DealAttackDamageTo(Unit target, Ability sourceAbility)
    {
        target.TakeDamage(new Damage(_stats[Stat.AttackDamage]), this, sourceAbility);
        foreach (StatusContext statusContext in _statusContexts.Values.ToList())
            statusContext.Status.OnDealAttackDamage(statusContext, target, _stats[Stat.AttackDamage]);
    }

    public void AddStatus(Status status, double time, ISourceUnitAndAbility? source) =>
        AddStatus(status, time, source?.Unit, source?.AbilityContext);

    public void AddStatus<T>
    (
        double time,
        Unit? sourceUnit,
        AbilityContext? sourceAbilityContext
    ) where T : Status, new()
    {
        AddStatus(Status.Instance<T>(), time, sourceUnit, sourceAbilityContext);
    }

    public void AddStatus<T>(double time, ISourceUnitAndAbility? source) where T : Status, new() =>
        AddStatus<T>(time, source?.Unit, source?.AbilityContext);

    public void AddStatus
    (
        Status status,
        double time,
        Unit? sourceUnit,
        AbilityContext? sourceAbilityContext
    )
    {
        if (time < 0)
            throw new ArgumentException("Time must not be negative");
        if (!_allowAbilityPassiveStatusManipulation && status is AbilityPassiveStatus)
            throw new InvalidOperationException("Ability passive statuses should not be added manually");
        if (IsRemoved) return;

        StatusContext context = new StatusContext
        {
            Id = Guid.NewGuid(),
            Status = status,
            SourceAbilityContext = sourceAbilityContext,
            Unit = this,
            SourceUnit = sourceUnit,
            ElapsedTime = 0,
            DisplayElapsedTime = 0,
            RemainingTime = time,
            Ordinal = _nextStatusOrdinal++,
            Services = _services
        };

        List<StatusContext> duplicates = _statusContexts.Values.Where(it => it.Status == status).ToList();
        if (duplicates.Count == 0)
        {
            _statusContexts[context.Id] = context;
        }
        else
        {
            switch (status.DuplicateResolution)
            {
                case DuplicateStatusResolution.Stack:
                    _statusContexts[context.Id] = context;
                    break;
                case DuplicateStatusResolution.Refresh:
                    RefreshDuplicateStatuses(context, duplicates.Single());
                    break;
                case DuplicateStatusResolution.StackAndRefresh:
                    _statusContexts[context.Id] = context;
                    RefreshDuplicateStatuses(context, duplicates);
                    break;
                case DuplicateStatusResolution.Replace:
                    RemoveStatus(duplicates.Single().Id);
                    _statusContexts[context.Id] = context;
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
            _statusContexts[target.Id] = target with
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
        if (!_statusContexts.TryGetValue(id, out StatusContext? statusContext)) return;
        if (!_allowAbilityPassiveStatusManipulation && statusContext.Status is AbilityPassiveStatus)
            throw new InvalidOperationException("Ability passive statuses should not be removed manually");
        if (IsRemoved) return;
        _statusContexts.Remove(id);
        UpdateStats();
    }

    public void RemoveAllStatuses<T>() where T : Status, new() => RemoveAllStatuses(Status.Instance<T>());

    public void RemoveAllStatuses(Status status)
    {
        if (IsRemoved) return;
        foreach ((Guid id, StatusContext context) in _statusContexts)
            if (context.Status == status)
                RemoveStatus(id);
    }

    public void Die()
    {
        if (IsRemoved) return;
        IsDead = true;
        Remove();
    }

    public bool IsAlliedTo(Unit other) => Faction != Faction.Neutral && other.Faction == Faction;

    private void UpdateHotbarAbilityList()
    {
        IEnumerable<Ability> itemHotbarAbilities = EquipmentInventory
            .Where(it => it.Value != null)
            .OrderBy(it => it.Key)
            .SelectMany(it => it.Value.Required.Item.HotbarAbilities);
        List<Ability> hotbarAbilities = _innateHotbarAbilities.Concat(itemHotbarAbilities).Distinct().ToList();

        Dictionary<AbilitySlot, AbilitySlotState> newStates = [];
        List<Ability> addedAbilities = [];
        List<Ability> removedAbilities = [];
        for (int i = 0; i < hotbarAbilities.Count; i++)
        {
            var slot = AbilitySlot.Hotbar(i);
            Ability ability = hotbarAbilities[i];
            AbilitySlotState? currentState = _abilitySlotStates.Values.SingleOrDefault(it => it.Ability == ability);
            newStates[slot] = currentState ?? new AbilitySlotState(ability);
            if (currentState == null)
                addedAbilities.Add(ability);
        }
        foreach ((AbilitySlot slot, AbilitySlotState state) in _abilitySlotStates)
        {
            if (slot is SpecialAbilitySlot)
                newStates[slot] = state;
            else if (!newStates.ContainsKey(slot))
                removedAbilities.Add(state.Ability);
        }
        _abilitySlotStates = newStates;

        addedAbilities.ForEach(OnAbilityAdded);
        removedAbilities.ForEach(OnAbilityRemoved);
    }

    private void UpdateSpecialAbilityList()
    {
        foreach (SpecialAbilitySlot slot in AbilitySlot.AllSpecial)
        {
            AbilitySlotState? currentState = _abilitySlotStates.GetValueOrDefault(slot);

            Ability? itemAbility = EquipmentInventory
                .OrderBy(it => it.Key)
                .Select(it => it.Value?.Item.SpecialAbilities.GetValueOrDefault(slot))
                .FirstOrDefault(it => it != null);
            if (itemAbility != null && currentState?.Ability != itemAbility)
            {
                if (currentState != null)
                {
                    _abilitySlotStates[slot] = currentState with { Ability = itemAbility};
                    OnAbilityRemoved(currentState.Ability);
                }
                else
                {
                    _abilitySlotStates[slot] = new AbilitySlotState(itemAbility);
                }
                OnAbilityAdded(itemAbility);
            }

            Ability? innateAbility = null;
            if
            (
                itemAbility == null &&
                _innateSpecialAbilities.TryGetValue(slot, out innateAbility) &&
                currentState?.Ability != innateAbility
            )
            {
                if (currentState != null)
                {
                    _abilitySlotStates[slot] = currentState with { Ability = innateAbility };
                    OnAbilityRemoved(currentState.Ability);
                }
                else
                {
                    _abilitySlotStates[slot] = new AbilitySlotState(innateAbility);
                }
                OnAbilityAdded(innateAbility);
            }

            if (itemAbility == null && innateAbility == null && currentState != null)
            {
                _abilitySlotStates.Remove(slot);
                OnAbilityRemoved(currentState.Ability);
            }
        }
    }

    private void OnAbilityAdded(Ability ability)
    {
        _commands.Clear();
        AbilityUseProgress = null;

        if (ability.PassiveStatus != null)
        {
            _allowAbilityPassiveStatusManipulation = true;
            AddStatus(ability.PassiveStatus, double.PositiveInfinity, this, GetPassiveAbilityContext(ability));
            _allowAbilityPassiveStatusManipulation = false;
        }
    }

    private void OnAbilityRemoved(Ability ability)
    {
        _commands.Clear();
        AbilityUseProgress = null;

        if (ability.PassiveStatus != null)
        {
            _allowAbilityPassiveStatusManipulation = true;
            RemoveAllStatuses(ability.PassiveStatus);
            _allowAbilityPassiveStatusManipulation = false;
        }
    }

    protected void AddInnateHotbarAbility<T>() where T : Ability, new() =>
        AddInnateHotbarAbility(Ability.Instance<T>());

    protected void AddInnateHotbarAbility(Ability ability)
    {
        if (IsRemoved) return;
        if (_innateHotbarAbilities.Contains(ability)) return;
        _innateHotbarAbilities.Add(ability);
        UpdateHotbarAbilityList();
    }

    protected void RemoveInnateHotbarAbility<T>() where T : Ability, new() =>
        RemoveInnateHotbarAbility(Ability.Instance<T>());

    protected void RemoveInnateHotbarAbility(Ability ability)
    {
        if (IsRemoved) return;
        if (_innateHotbarAbilities.Remove(ability))
            UpdateHotbarAbilityList();
    }

    protected void SetInnateSpecialAbility<T>(SpecialAbilitySlot slot) where T : Ability, new() =>
        SetInnateSpecialAbility(slot, Ability.Instance<T>());

    protected void SetInnateSpecialAbility(SpecialAbilitySlot slot, Ability ability)
    {
        if (IsRemoved) return;
        _innateSpecialAbilities[slot] = ability;
        UpdateSpecialAbilityList();
    }

    protected void RemoveInnateSpecialAbility(SpecialAbilitySlot slot)
    {
        if (IsRemoved) return;
        if (_innateSpecialAbilities.Remove(slot))
            UpdateSpecialAbilityList();
    }
}
