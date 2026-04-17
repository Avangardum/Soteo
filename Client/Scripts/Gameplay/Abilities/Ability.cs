using System.Collections.Immutable;
using System.Reflection;
using Soteo.Gameplay.Enums;
using Soteo.Shared;
using Soteo.Shared.Enums;
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
                .Select(it =>
                {
                    if (!it.BaseTypes.Any(b => b.IsGenericType && b.GetGenericTypeDefinition() == typeof(Ability<>)))
                        throw new InvalidOperationException($"{it.Name} should inherit Ability<{it.Name}>");
                    Type genericAbilityType = typeof(Ability<>).MakeGenericType(it);
                    return (Ability)genericAbilityType.GetProperty(nameof(Ability<>.Instance))!.GetValue(null);
                })
                .ToImmutableList();
            return field;
        }
    }
    
    public virtual int MaxLevel => 1;
    public virtual Scalable<float> HealthCost => 0;
    public virtual Scalable<float> ManaCost => 0;
    public virtual Scalable<float> Cooldown => 0;
    public virtual Scalable<float> Range => 0;
    public virtual Scalable<float> AngularRangeDeg => 30;
    public virtual Scalable<float> CastTimeSec => 0;
    public virtual AbilityTargetFlags TargetFlags => AbilityTargetFlags.Untargeted;
    
    /// <summary>
    /// Called when an ability cast is completed and it takes effect.
    /// This should be called only immediately after non-strict validation succeeds.
    /// </summary>
    public virtual void OnCasted(AbilityCastContext context)
    {
        AbilityValidationResult validationResult = Validate(context);
        if (validationResult != AbilityValidationResult.Ok)
            throw new InvalidOperationException($"Ability validation failed: {validationResult}");
        if (HealthCost[context.Level] > 0) context.Caster.SpendHealth(HealthCost[context.Level], this);
        if (ManaCost[context.Level] > 0) context.Caster.SpendMana(ManaCost[context.Level], this);
    }
    
    /// <summary>
    /// Checks whether an ability can be cast and returns a reason if not.
    /// Strict mode is used to determine whether cast can be initiated.
    /// Non-strict mode is used to determine whether in-progress cast should be interrupted.
    /// </summary>
    public virtual AbilityValidationResult Validate(AbilityCastContext context, bool strict = true)
    {
        if (context.Level < 1 || context.Level > MaxLevel) return AbilityValidationResult.InvalidLevel;
        
        if (context.Cooldown > 0) return AbilityValidationResult.Cooldown;
        
        if (!TargetFlags.HasFlag(AbilityTargetFlags.Untargeted) &&
            context.TargetPoint == null && context.TargetUnit == null)
        {
            return AbilityValidationResult.InvalidTarget;
        }
        if (context.TargetPoint != null && context.TargetUnit != null)
            return AbilityValidationResult.InvalidTarget;
        if (!TargetFlags.HasFlag(AbilityTargetFlags.Point) && context.TargetPoint != null)
            return AbilityValidationResult.InvalidTarget;
        if (!TargetFlags.HasFlag(AbilityTargetFlags.Unit) && context.TargetUnit != null)
            return AbilityValidationResult.InvalidTarget;
        if (TargetFlags.HasFlag(AbilityTargetFlags.HasDirection) != context.TargetDirection.HasValue)
            return AbilityValidationResult.InvalidTarget;
        if (TargetFlags.HasFlag(AbilityTargetFlags.HasShard) != context.TargetShardId.HasValue)
            return AbilityValidationResult.InvalidTarget;
        
        if (context.Caster.Stats[Stat.CurrentHealth] <= HealthCost[context.Level])
            return AbilityValidationResult.NotEnoughHealth;
        if (context.Caster.Stats[Stat.CurrentMana] < ManaCost[context.Level])
            return AbilityValidationResult.NotEnoughMana;
        
        if ((context.TargetPoint ?? context.TargetUnit?.Position) is Vector2 targetPosition)
        {
            Vector2 deltaPosition = targetPosition - context.Caster.Position;
            float rangeMultiplier = strict ? 1 : 1.5f;
            if (deltaPosition.Length() > Range[context.Level] * rangeMultiplier)
                return AbilityValidationResult.OutOfRange;
            
            float deltaAzimuth =
                SoteoMath.ModularDelta(context.Caster.Azimuth, SoteoMath.DirectionToAzimuth(deltaPosition), 360);
            if (Mathf.Abs(deltaAzimuth) > AngularRangeDeg[context.Level] * rangeMultiplier)
                return AbilityValidationResult.OutOfRange;
        }

        return AbilityValidationResult.Ok;
    }
}

public abstract class Ability<T> : Ability where T : Ability<T>, new()
{
    public static T Instance { get; } = new();
}