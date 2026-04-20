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
    public virtual AbilityTargetFlags TargetFlags => AbilityTargetFlags.Untargeted;
    
    // Static values define what is shown in ability description. They are constant and independent of context.
    public virtual Scalable<float> StaticHealthCost => 0;
    public virtual Scalable<float> StaticManaCost => 0;
    public virtual Scalable<float> StaticCooldown => 0;
    public virtual Scalable<float> StaticRange => 0;
    public virtual Scalable<float> StaticAngularRange => 30;
    public virtual Scalable<float> StaticUseTime => 0;
   
    // Dynamic values are context dependent values declared by an ability before applying status effect modifiers.
    // By default, they are same as static values, but if an ability has a value that can't be declared statically, it
    // should override the matching dynamic value method, in which case the static value is used for ability description
    // only and should be 0 to hide it in most cases.
    protected virtual float DynamicHealthCost(AbilityUseContext context) => StaticHealthCost[context.Level];
    protected virtual float DynamicManaCost(AbilityUseContext context) => StaticManaCost[context.Level];
    protected virtual float DynamicCooldown(AbilityUseContext context) => StaticCooldown[context.Level];
    protected virtual float DynamicRange(AbilityUseContext context) => StaticRange[context.Level];
    protected virtual float DynamicAngularRange(AbilityUseContext context) => StaticAngularRange[context.Level];
    protected virtual float DynamicUseTime(AbilityUseContext context) => StaticUseTime[context.Level];
    
    // Unprefixed values are values after applying status effect modifiers and are used in actual gameplay.
    public float HealthCost(AbilityUseContext context) => DynamicHealthCost(context);
    public float ManaCost(AbilityUseContext context) => DynamicManaCost(context);
    public float Cooldown(AbilityUseContext context) => DynamicCooldown(context);
    public float Range(AbilityUseContext context) => DynamicRange(context);
    public float AngularRange(AbilityUseContext context) => DynamicAngularRange(context);
    public float UseTime(AbilityUseContext context) => DynamicUseTime(context);
    
    /// <summary>
    /// Called when an ability use is completed and it takes effect.
    /// This should be called only immediately after non-strict validation succeeds.
    /// </summary>
    public virtual void TakeEffect(AbilityUseContext context)
    {
        AbilityValidationResult validationResult = Validate(context);
        if (validationResult != AbilityValidationResult.Ok)
            throw new InvalidOperationException($"Ability validation failed: {validationResult}");
        context.Caster.SpendHealth(HealthCost(context), this);
        context.Caster.SpendMana(ManaCost(context), this);
    }
    
    /// <summary>
    /// Checks whether an ability can be used and returns a reason if not.
    /// Strict mode is used to determine whether use can be initiated.
    /// Non-strict mode is used to determine whether in-progress use should be interrupted.
    /// </summary>
    public virtual AbilityValidationResult Validate(AbilityUseContext context, bool strict = true)
    {
        if (context.Level < 1 || context.Level > MaxLevel) return AbilityValidationResult.InvalidLevel;
        
        AbilityValidationResult targetValidationResult = ValidateTarget(context);
        if (targetValidationResult != AbilityValidationResult.Ok) return targetValidationResult;
        
        AbilityValidationResult costValidationResult = ValidateCost(context);
        if (costValidationResult != AbilityValidationResult.Ok) return costValidationResult;
        
        AbilityValidationResult rangeValidationResult = ValidateRange(context, strict);
        if (rangeValidationResult != AbilityValidationResult.Ok) return rangeValidationResult;

        return AbilityValidationResult.Ok;
    }
    
    private AbilityValidationResult ValidateTarget(AbilityUseContext context)
    {
        if (!TargetFlags.HasFlag(AbilityTargetFlags.Untargeted) &&
            context.TargetPosition == null && context.TargetUnit == null)
        {
            return AbilityValidationResult.InvalidTarget;
        }
        if (context.TargetPosition != null && context.TargetUnit != null)
            return AbilityValidationResult.InvalidTarget;
        if (!TargetFlags.HasFlag(AbilityTargetFlags.Position) && context.TargetPosition != null)
            return AbilityValidationResult.InvalidTarget;
        if (!TargetFlags.HasFlag(AbilityTargetFlags.Unit) && context.TargetUnit != null)
            return AbilityValidationResult.InvalidTarget;
        if (TargetFlags.HasFlag(AbilityTargetFlags.HasDirection) != context.TargetDirection.HasValue)
            return AbilityValidationResult.InvalidTarget;
        if (TargetFlags.HasFlag(AbilityTargetFlags.HasShard) != context.TargetShardId.HasValue)
            return AbilityValidationResult.InvalidTarget;
        return AbilityValidationResult.Ok;
    }
    
    private AbilityValidationResult ValidateCost(AbilityUseContext context)
    {
        if (context.Caster.Stats[Stat.CurrentHealth] <= HealthCost(context))
            return AbilityValidationResult.NotEnoughHealth;
        if (context.Caster.Stats[Stat.CurrentMana] < ManaCost(context))
            return AbilityValidationResult.NotEnoughMana;
        return AbilityValidationResult.Ok;
    }
    
    private AbilityValidationResult ValidateRange(AbilityUseContext context, bool strict)
    {
        if ((context.TargetPosition ?? context.TargetUnit?.Position) is Vector2 targetPosition &&
            targetPosition != context.Caster.Position)
        {
            Vector2 deltaPosition = targetPosition - context.Caster.Position;
            float rangeMultiplier = strict ? 1 : 1.5f;
            if (deltaPosition.Length() > Range(context) * rangeMultiplier)
                return AbilityValidationResult.OutOfRange;
            
            float deltaAzimuth =
                SoteoMath.ModularDelta(context.Caster.Azimuth, SoteoMath.DirectionToAzimuth(deltaPosition), 360);
            if (Mathf.Abs(deltaAzimuth) > AngularRange(context) * rangeMultiplier)
                return AbilityValidationResult.OutOfRange;
        }
        
        return AbilityValidationResult.Ok;
    }
}

public abstract class Ability<T> : Ability where T : Ability<T>, new()
{
    public static T Instance { get; } = new();
}