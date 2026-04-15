using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public abstract class Ability
{
    public virtual int MaxLevel => 1;
    public virtual Scalable<int> HealthCost => 0;
    public virtual Scalable<int> ManaCost => 0;
    public virtual Scalable<float> Cooldown => 0;
    public abstract Scalable<float> CastRange { get; }
    public virtual Scalable<float> CastTimeSeconds => 0;
}

public abstract class Ability<T> : Ability where T : Ability<T>, new()
{
    public static T Instance { get; } = new();
}