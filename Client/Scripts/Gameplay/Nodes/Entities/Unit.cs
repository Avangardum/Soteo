using System.Collections.Immutable;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Commands;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Entities;

public class Unit : KinematicBody2D, IEntity
{
    private Line2D _azimuthLine = null!;
    
    private Queue<ICommand> Commands { get; } = [];
    
    private Dictionary<Stat, float> StatsInternal { get; } = [];
    public IReadOnlyDictionary<Stat, float> Stats => StatsInternal;
    
    protected Dictionary<AbilitySlot, AbilityState> AbilityStatesInternal { get; } = [];
    public ICovariantReadOnlyDictionary<AbilitySlot, IReadOnlyAbilityState> AbilityStates =>
        AbilityStatesInternal.AsCovariant();

    public AbilitySlot? CurrentAbilitySlot { get; private set; }
    public float CurrentAbilityProgressSeconds { get; private set; } = -1;
    
    public Guid Id { get; set; }

    public Faction Faction { get; set; }
    
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
            CurrentAbilityProgressSeconds = CurrentAbilityProgressSeconds
        };
    }

    public void ReplicateSnapshot(EntitySnapshot snapshot)
    {
        if (snapshot.Position != null) Position = snapshot.Position.Value;
        if (snapshot.Azimuth != null) Azimuth = snapshot.Azimuth.Value;
        foreach ((Stat stat, float value) in snapshot.Stats) StatsInternal[stat] = value;
        foreach ((AbilitySlot slot, IReadOnlyAbilityState state) in snapshot.AbilityStates)
            AbilityStatesInternal[slot] = new AbilityState(state);
        if (snapshot.CurrentAbilitySlot != null) CurrentAbilitySlot = snapshot.CurrentAbilitySlot.Value;
        if (snapshot.CurrentAbilityProgressSeconds == -1) CurrentAbilitySlot = null;
        if (snapshot.CurrentAbilityProgressSeconds != null)
            CurrentAbilityProgressSeconds = snapshot.CurrentAbilityProgressSeconds.Value;
    }

    public override void _Ready()
    {
        _azimuthLine = GetNode<Line2D>("AzimuthLine");
        
        StatsInternal[Stat.CurrentHealth] = StatsInternal[Stat.MaxHealth] = 1000;
        StatsInternal[Stat.CurrentMana] = StatsInternal[Stat.MaxMana] = 1000;
        StatsInternal[Stat.MoveSpeed] = 50;
        StatsInternal[Stat.TurnSpeed] = 360;
    }

    public override void _PhysicsProcess(float deltaTime)
    {
        if (!IsServer) return;
        
        foreach (AbilityState abilityState in AbilityStatesInternal.Values)
            abilityState.Cooldown = Mathf.Max(abilityState.Cooldown - deltaTime, 0);
        ExecuteCommands(deltaTime);
    }
    
    private void ExecuteCommands(float deltaTime)
    {
        float remainingDeltaTime = deltaTime;
        while (Commands.Count > 0 && remainingDeltaTime > 0)
        {
            switch (Commands.Peek())
            {
                case LookCommand command:
                    LookAtPosition(command.Position, ref remainingDeltaTime);
                    break;
                case MoveCommand command:
                    LookAtPosition(command.Position, ref remainingDeltaTime);
                    MoveToPosition(command.Position, ref remainingDeltaTime);
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

    public void SetCommand(ICommand command)
    {
        Commands.Clear();
        Commands.Enqueue(command);
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
    
    public void Heal(float amount, Unit source, Ability ability)
    {
        StatsInternal[Stat.CurrentHealth] =
            Mathf.Clamp(StatsInternal[Stat.CurrentHealth] + amount, 0, Stats[Stat.MaxHealth]); 
    }
    
    public void RestoreMana(float amount, Unit source, Ability ability)
    {
        StatsInternal[Stat.CurrentMana] =
            Mathf.Clamp(StatsInternal[Stat.CurrentMana] + amount, 0, Stats[Stat.MaxMana]);
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