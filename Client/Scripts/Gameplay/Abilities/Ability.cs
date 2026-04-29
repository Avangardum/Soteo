using System.Collections.Immutable;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Statuses;
using Soteo.Gameplay.Util;
using Soteo.Shared;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Abilities;

public abstract class Ability
{
    private static readonly ImmutableDictionary<Type, Ability> InstancesByType;

    public static ImmutableList<Ability> All { get; }

    public static T Instance<T>() where T : Ability => (T)InstancesByType[typeof(T)];

    static Ability()
    {
        All = TypeLocator.InstanceAllSubclasses<Ability>();
        InstancesByType = All.ToImmutableDictionary(it => it.GetType(), it => it);
    }
    
    public int Id => All.IndexOf(this);
    
    public virtual int MaxLevel => 1;
    public virtual Status? PassiveStatus => null;
    public virtual CanTarget Targeting => CanTarget.Passive;
    public virtual string Animation => "Attack Right";
    public virtual bool LoopAnimation => false;
    
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
    protected virtual float DynamicHealthCost(AbilityContext context) => StaticHealthCost[context.Level];
    protected virtual float DynamicManaCost(AbilityContext context) => StaticManaCost[context.Level];
    protected virtual float DynamicCooldown(AbilityContext context) => StaticCooldown[context.Level];
    protected virtual float DynamicRange(AbilityContext context) => StaticRange[context.Level];
    protected virtual float DynamicAngularRange(AbilityContext context) => StaticAngularRange[context.Level];
    protected virtual float DynamicUseTime(AbilityContext context) => StaticUseTime[context.Level];
    
    // Unprefixed values are values after applying status effect modifiers and are used in actual gameplay.
    public float HealthCost(AbilityContext context) => DynamicHealthCost(context);
    public float ManaCost(AbilityContext context) => DynamicManaCost(context);
    public float Cooldown(AbilityContext context) => DynamicCooldown(context);
    public float Range(AbilityContext context) => DynamicRange(context);
    public float AngularRange(AbilityContext context) => DynamicAngularRange(context);
    public float UseTime(AbilityContext context) => DynamicUseTime(context);
    
    /// <summary>
    /// Called when an ability use is completed and it takes effect.
    /// This should be called only immediately after non-strict validation succeeds.
    /// </summary>
    public virtual void TakeEffect(AbilityContext context)
    {
        AbilityValidationResult validationResult = Validate(context);
        if (validationResult != AbilityValidationResult.Ok)
            throw new InvalidOperationException($"Ability validation failed: {validationResult}");
        context.User.SpendHealth(HealthCost(context), this);
        context.User.SpendMana(ManaCost(context), this);
    }

    public virtual void OnProjectileHit(AbilityContext context) { }
    
    /// <summary>
    /// Checks whether an ability can be used and returns a reason if not.
    /// Strict mode is used to determine whether use can be initiated.
    /// Non-strict mode is used to determine whether in-progress use should be interrupted.
    /// </summary>
    public virtual AbilityValidationResult Validate(AbilityContext context, bool strict = true)
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
    
    private AbilityValidationResult ValidateTarget(AbilityContext context)
    {
        if (!Targeting.HasFlag(CanTarget.Nothing) && context.TargetPosition == null && context.TargetUnit == null)
            return AbilityValidationResult.InvalidTarget;
        if (context.TargetPosition != null && context.TargetUnit != null)
            return AbilityValidationResult.InvalidTarget;
        if (!Targeting.HasFlag(CanTarget.Position) && context.TargetPosition != null)
            return AbilityValidationResult.InvalidTarget;
        if (context.TargetUnit != null)
        {
            AbilityValidationResult targetUnitValidationResult = ValidateTargetUnit(context);
            if (targetUnitValidationResult != AbilityValidationResult.Ok) return targetUnitValidationResult;
        }
        if (Targeting.HasFlag(CanTarget.WithDirection) != context.TargetDirection.HasValue)
            return AbilityValidationResult.InvalidTarget;
        if (Targeting.HasFlag(CanTarget.WithShard) != context.TargetShardId.HasValue)
            return AbilityValidationResult.InvalidTarget;
        return AbilityValidationResult.Ok;
    }
    
    private AbilityValidationResult ValidateTargetUnit(AbilityContext context)
    {
        if (context.User.IsAlliedTo(context.TargetUnit.Required))
        {
            if (!Targeting.HasFlag(CanTarget.Ally)) return AbilityValidationResult.InvalidTarget;
        }
        else
        {
            if (!Targeting.HasFlag(CanTarget.Enemy)) return AbilityValidationResult.InvalidTarget;
        }
        
        // todo validate character / building
        
        return AbilityValidationResult.Ok;
    }
    
    private AbilityValidationResult ValidateCost(AbilityContext context)
    {
        if (HealthCost(context) > 0 && context.User.Stats[Stat.CurrentHealth] <= HealthCost(context))
            return AbilityValidationResult.NotEnoughHealth;
        if (context.User.Stats[Stat.CurrentMana] < ManaCost(context))
            return AbilityValidationResult.NotEnoughMana;
        return AbilityValidationResult.Ok;
    }
    
    private AbilityValidationResult ValidateRange(AbilityContext context, bool strict)
    {
        if ((context.TargetPosition ?? context.TargetUnit?.Position) is Vector2 targetPosition &&
            targetPosition != context.User.Position)
        {
            Vector2 deltaPosition = targetPosition - context.User.Position;
            float rangeMultiplier = strict ? 1 : 1.5f;
            if (deltaPosition.Length() > Range(context) * rangeMultiplier)
                return AbilityValidationResult.OutOfRange;
            
            float deltaAzimuth =
                SoteoMath.ModularDelta(context.User.Azimuth, SoteoMath.DirectionToAzimuth(deltaPosition), 360);
            if (Mathf.Abs(deltaAzimuth) > AngularRange(context) * rangeMultiplier)
                return AbilityValidationResult.OutOfAngularRange;
        }
        
        return AbilityValidationResult.Ok;
    }
}