using System.Collections.Immutable;
using System.Reflection;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Abilities;

public abstract class Ability
{
    public static ImmutableList<Ability> All
    {
        get
        {
            field ??= Assembly.GetExecutingAssembly().DefinedTypes
                .Where(it => !it.IsAbstract && it.IsAssignableTo(typeof(Ability)))
                .OrderBy(it => it.FullName)
                .Select(it => (Ability)
                    typeof(Ability<>).MakeGenericType(it).GetProperty(nameof(Ability<>.Instance))!.GetValue(null))
                .ToImmutableList();
            return field;
        }
    }
    
    public virtual int MaxLevel => 1;
    public virtual Scalable<float> HealthCost => 0;
    public virtual Scalable<float> ManaCost => 0;
    public virtual Scalable<float> Cooldown => 0;
    public abstract Scalable<float> CastRange { get; }
    public virtual Scalable<float> CastTimeSeconds => 0;
}

public abstract class Ability<T> : Ability where T : Ability<T>, new()
{
    public static T Instance { get; } = new();
}