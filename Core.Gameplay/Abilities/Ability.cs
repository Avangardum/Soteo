using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Statuses;
using Soteo.Core.Shared;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Abilities;

public abstract class Ability
{
    private static readonly ImmutableDictionary<Type, Ability> InstancesByType;

    public static ImmutableList<Ability> All { get; }

    public static T Instance<T>() where T : Ability
    {
        if (InstancesByType.TryGetValue(typeof(T), out Ability? instance))
            return (T)instance;
        throw ExceptionFactory.TypeNotFound(typeof(T));
    }

    static Ability()
    {
        All = TypeLocator.InstanceSubclassesOf<Ability>();
        InstancesByType = All.ToImmutableDictionary(it => it.GetType(), it => it);
    }
    
    public int Id => All.IndexOf(this);
    
    public virtual string Name =>
        GetType().Name.ReplaceRegex("Ability$", "").PascalCaseToCapitalizedText();
    
    public virtual int MaxLevel => 1;
    public virtual Status? PassiveStatus => null;
    public virtual double? PassiveTickInterval => null;
    public virtual CanTarget Targeting => CanTarget.Passive;
    
    public virtual string Animation => "Ability";
    public virtual bool LoopAnimation => false;
    
    /// <summary>
    /// Path to the ability icon relative to res://Textures/Icons, without an extension
    /// </summary>
    public virtual string IconPath => "Placeholder";
    
    // Static values define what is shown in the ability description. They are constant and independent of context.
    public virtual Scalable<double> StaticHealthCost => 0;
    public virtual Scalable<double> StaticManaCost => 0;
    public virtual Scalable<double> StaticCooldown => 0;
    public virtual Scalable<double> StaticRange => 0;
    public virtual Scalable<double> StaticAngularRange => 30;
    public virtual Scalable<double> StaticUseTime => 0;
   
    // Dynamic values are context dependent values declared by the ability before applying status effect modifiers.
    // By default, they are same as static values, but if an ability has a value that can't be declared statically, it
    // should override the matching dynamic value method, in which case the static value is used for ability description
    // only and should be 0 to hide it in most cases.
    protected virtual double DynamicHealthCost(AbilityContext context) => StaticHealthCost[context.Level];
    protected virtual double DynamicManaCost(AbilityContext context) => StaticManaCost[context.Level];
    protected virtual double DynamicCooldown(AbilityContext context) => StaticCooldown[context.Level];
    protected virtual double DynamicRange(AbilityContext context) => StaticRange[context.Level];
    protected virtual double DynamicAngularRange(AbilityContext context) => StaticAngularRange[context.Level];
    protected virtual double DynamicUseTime(AbilityContext context) => StaticUseTime[context.Level];
    
    // Unprefixed values are values after applying status effect modifiers and are used in actual gameplay.
    public double HealthCost(AbilityContext context) => DynamicHealthCost(context);
    public double ManaCost(AbilityContext context) => DynamicManaCost(context);
    public double Cooldown(AbilityContext context) => DynamicCooldown(context);
    public double Range(AbilityContext context) => DynamicRange(context);
    public double AngularRange(AbilityContext context) => DynamicAngularRange(context);
    public double UseTime(AbilityContext context) => DynamicUseTime(context);
    
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
            AbilityValidationResult targetUnitValidationResult = ValidateTargetUnit(context.User, context.TargetUnit);
            if (targetUnitValidationResult != AbilityValidationResult.Ok)
                return targetUnitValidationResult;
        }
        if (Targeting.HasFlag(CanTarget.WithDirection) != context.TargetDirection.HasValue)
            return AbilityValidationResult.InvalidTarget;
        if (Targeting.HasFlag(CanTarget.WithShard) != context.TargetShardId.HasValue)
            return AbilityValidationResult.InvalidTarget;
        return AbilityValidationResult.Ok;
    }
    
    private AbilityValidationResult ValidateTargetUnit(Unit user, Unit target)
    {
        if (user.IsAlliedTo(target))
        {
            if (!Targeting.HasFlag(CanTarget.Ally))
                return AbilityValidationResult.InvalidTarget;
        }
        else
        {
            if (!Targeting.HasFlag(CanTarget.Enemy))
                return AbilityValidationResult.InvalidTarget;
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
        if 
        (
            (context.TargetPosition ?? context.TargetUnit?.Position) is Vector2 targetPosition &&
            targetPosition != context.User.Position
        )
        {
            Vector2 deltaPosition = targetPosition - context.User.Position;
            double rangeMultiplier = strict ? 1 : 1.5f;
            if (deltaPosition.Length() > Range(context) * rangeMultiplier)
                return AbilityValidationResult.OutOfRange;
            
            double deltaAzimuth =
                Maths.ModularDelta(context.User.Azimuth, Maths.DirectionToAzimuth(deltaPosition), 360);
            if (Math.Abs(deltaAzimuth) > AngularRange(context) * rangeMultiplier)
                return AbilityValidationResult.OutOfAngularRange;
        }
        
        return AbilityValidationResult.Ok;
    }
    
    public string Description(ILocalizer localizer, int? level = null)
    {
        string formatKey = GetType().Name.PascalCaseToSnakeCase().ToUpper() + "_DESCRIPTION";
        string format = localizer.GetString(formatKey);
        
        return format
            .PassTo(it => FillDescriptionProperties(it, level))
            .PassTo(it => FillDescriptionPluralization(it, localizer)) +
            DescriptionFooter(level);
    }
    
    private string FillDescriptionProperties(string value, int? level)
    {
        // example: {Duration:N2}
        const string propertyRegex = @"\{([A-Za-z0-9_]+)(?:\:([^\{\}\|\:]+))?\}";
        
        return value.ReplaceRegex
        (
            propertyRegex,
            match =>
            {
                string propertyName = match.Groups[1].Value;
                string? format = match.Groups[2].NullableValue;
                object? propertyValue = GetType().GetProperty(propertyName)?.GetValue(this);
                return propertyValue switch
                {
                    null => "ERROR",
                    Scalable s => s.ToBbcode(level, format),
                    IFormattable f => f.ToString(format, CultureInfo.CurrentCulture),
                    _ => propertyValue.ToString()
                };
            }
        );
    }
    
    private string FillDescriptionPluralization(string value, ILocalizer localizer)
    {
        // example: {Duration|second|seconds}
        const string pluralisationRegex = @"\{([A-Za-z0-9_]+)(?:\|([^\{\}\|]+))+\}";
        
        return value.ReplaceRegex
        (
            pluralisationRegex,
            match =>
            {
                string propertyName = match.Groups[1].Value;
                object? propertyValue = GetType().GetProperty(propertyName)?.GetValue(this);
                double? doubleValue = propertyValue is IConvertible ? Convert.ToDouble(propertyValue) : null;
                if (doubleValue == null) return "ERROR";
                int pluralizationIndex = localizer.GetPluralisationIndex(doubleValue);
                return match.Groups[2].Captures[pluralizationIndex].Value;
            }
        );
    }
    
    private string DescriptionFooter(int? level)
    {
        List<string> parts = [];
        if (StaticHealthCost.Any(it => it > 0))
            parts.Add("[img]res://Textures/Icons/BbCode/Health.png[/img] " + StaticHealthCost.ToBbcode(level));
        if (StaticManaCost.Any(it => it > 0))
            parts.Add("[img]res://Textures/Icons/BbCode/Mana.png[/img] " + StaticManaCost.ToBbcode(level));
        if (StaticCooldown.Any(it => it > 0))
            parts.Add("[img]res://Textures/Icons/BbCode/Cooldown.png[/img] " + StaticCooldown.ToBbcode(level));
        if (StaticRange.Any(it => it > 0))
            parts.Add("[img]res://Textures/Icons/BbCode/Range.png[/img] " + StaticRange.ToBbcode(level));
        return parts.Count == 0 ? "" : "\n" + string.Join("\n", parts);
    }
}
