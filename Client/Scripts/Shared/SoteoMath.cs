using System.Collections.Immutable;
using Soteo.Shared.Extensions;
using static Godot.Mathf;

namespace Soteo.Shared;

public static class SoteoMath
{
    public static double DirectionToAzimuth(Vector2 direction) =>
        Rad2Deg(Mathf.PosMod(Atan2(direction.y, direction.x) + Pi / 2, 2 * Pi));
    
    public static Vector2 AzimuthToDirection(double azimuth)
    {
        double atan2 = Deg2Rad(azimuth) - Pi / 2;
        return Vector2.New(Math.Cos(atan2), Math.Sin(atan2));
    }
    
    /// <summary>
    /// Linear interpolation, but for modular arithmetics with possible wrapping around zero. Useful for azimuth
    /// interpolation.<br />
    /// Examples: f(350, 30, 0.5, 360) = 20, f(100, 80, 0.5, 360) = 90.
    /// </summary>
    public static double ModularLerp(double from, double to, double weight, double modulo) =>
        PosMod(from + ModularDelta(from, to, modulo) * weight, modulo);

    /// <summary>
    /// Linear interpolation, but for modular arithmetics with possible wrapping around zero and only moving in positive
    /// direction. Useful for animations.<br />
    /// Examples: f(350, 30, 0.5, 360) = 20, f(100, 80, 0.5, 360) = 270.
    /// </summary>
    public static double ModularLerpPositive(double from, double to, double weight, double modulo) =>
        PosMod(from + ModularDeltaPositive(from, to, modulo) * weight, modulo);

    /// <summary>
    /// Find difference, but for modular arithmetics with possible wrapping around zero. Useful for azimuth.<br />
    /// Examples: f(350, 30, 360) = +40, f(100, 80, 360) = -20.
    /// </summary>
    public static double ModularDelta(double from, double to, double modulo)
    {
        if (from < 0 || from >= modulo) throw new ArgumentException();
        if (to < 0 || to >= modulo) throw new ArgumentException();
        
        double delta = to - from;
        if (delta > modulo / 2) delta -= modulo;
        else if (delta < -modulo / 2) delta += modulo;
        return delta;
    }
    
    /// <summary>
    /// Find difference, but for modular arithmetics with possible wrapping around zero and only moving in positive
    /// direction.<br />
    /// Examples: f(350, 30, 360) = +40, f(100, 80, 360) = 340.
    /// </summary>
    public static double ModularDeltaPositive(double from, double to, double modulo)
    {
        if (from < 0 || from >= modulo) throw new ArgumentException();
        if (to < 0 || to >= modulo) throw new ArgumentException();
        
        double delta = to - from;
        if (delta < 0) delta += modulo;
        return delta;
    }
    
    public static T? InterpolateNullable<T>(T? from, T? to, Func<T, T, T> interpolate) where T : struct
    {
        if (from == null || to == null) return to;
        return interpolate(from.Value, to.Value);
    }
    
    public static T? InterpolateNullable<T>(T? from, T? to, double weight, Func<T, T, double, T> interpolate)
        where T : struct
    {
        if (from == null || to == null) return to;
        return interpolate(from.Value, to.Value, weight);
    }

    public static T? InterpolateNullable<T>(T? from, T? to, Func<T, T, T> interpolate) where T : class
    {
        if (from == null || to == null) return to;
        return interpolate(from, to);
    }
    
    public static T? InterpolateNullable<T>(T? from, T? to, double weight, Func<T, T, double, T> interpolate)
        where T : class
    {
        if (from == null || to == null) return to;
        return interpolate(from, to, weight);
    }
    
    public static IReadOnlyDictionary<TKey, TValue> InterpolateDictionary<TKey, TValue>
    (
        IReadOnlyDictionary<TKey, TValue> from,
        IReadOnlyDictionary<TKey, TValue> to,
        double weight,
        Func<TValue, TValue, double, TValue> interpolateValue
    ) where TKey : notnull
    {
        return to.ToImmutableDictionary(it => it.Key, it =>
        {
            TValue t = it.Value;
            if (!from.TryGetValue(it.Key, out TValue? f)) return t;
            return interpolateValue(f, t, weight);
        });
    }
    
    public static double LerpDecrease(double from, double to, double weight) =>
        from > to ? Lerp(from, to, weight) : to;
    
    public static double LerpIncrease(double from, double to, double weight) =>
        from < to ? Lerp(from, to, weight) : to;
    
    public static double Lerp(double from, double to, double weight) =>
        from + weight * (to - from);
    
    public static double InverseLerp(double from, double to, double value) =>
        to == from ? 0.5f : (value - from) / (to - from);
    
    public static int PosMod(long value, int modulo) =>
        (int)(value >= 0 ? value % modulo : value % modulo + modulo);
    
    public static double PosMod(double value, double modulo) =>
        value >= 0 ? value % modulo : value % modulo + modulo;
    
    public static float Log(float value, float newBase) => (float)Math.Log(value, newBase);
    
    /// <summary>
    /// Round a value to the nearest noninteger multiple of 0.5
    /// </summary>
    public static double RoundToNonIntHalf(double value) => Math.Floor(value - 0.5f) + 0.5f;
    
    public static double RoundToMultipleOf(double multipleOf, double value) =>
        multipleOf == 0 ? 0 : Math.Round(value / multipleOf) * multipleOf;
    
    /// <summary>
    /// Round a value to a multiple of a given number plus half of that number.
    /// For example, specifying multipleOf: 10 rounds to one of the following: ..., -15, -5, 5, 15, ... 
    /// </summary>
    public static double RoundToMultipleOfPlusHalf(double multipleOf, double value) =>
        multipleOf == 0 ? 0 : RoundToNonIntHalf(value / multipleOf) * multipleOf;
    
    public static double Clamp(double value, double min, double max)
    {
        if (min > max) throw new ArgumentException("Min was greater than max");
        return Math.Min(Math.Max(value, min), max);
    }
    
    public static int FloorToInt(double value) => (int)Math.Floor(value);
    
    public static int CeilToInt(double value) => (int)Math.Ceiling(value);
    
    public static double Rad2Deg(double radians) => radians * 180 / Math.PI;
    
    public static double Deg2Rad(double degrees) => degrees * Math.PI / 180;
    
    public static bool IsMultipleOf(double factor, double value, double tolerance = 0.001)
    {
        double mod = PosMod(value, factor);
        return mod < tolerance || mod > factor - tolerance;
    }
}