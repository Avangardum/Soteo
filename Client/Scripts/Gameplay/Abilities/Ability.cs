using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class Ability
{
    public Unit Owner { get; }
    public int CurrentLevel { get; set; } = 1;
    public float CurrentCooldown { get; set; }
    
    public virtual int MaxLevel => 1;
    public virtual int HealthCost => 0;
    public virtual int ManaCost => 0;
    public virtual float Cooldown => 0;
    public abstract float CastRange { get; }
    public virtual float CastTimeSeconds => 0;

    protected Ability(Unit owner)
    {
        Owner = owner;
    }
}