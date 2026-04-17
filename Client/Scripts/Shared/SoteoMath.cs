namespace Soteo.Shared;

public static class SoteoMath
{
    public static float DirectionToAzimuth(Vector2 direction) =>
        Mathf.Rad2Deg(Mathf.PosMod(Mathf.Atan2(direction.y, direction.x) + Mathf.Pi / 2, 2 * Mathf.Pi));
    
    public static Vector2 AzimuthToDirection(float azimuth)
    {
        float atan2 = Mathf.Deg2Rad(azimuth) - Mathf.Pi / 2;
        return new Vector2(Mathf.Cos(atan2), Mathf.Sin(atan2));
    }
    
    /// <summary>
    /// Linear interpolation, but for modular arithmetics with possible wrapping around zero. Useful for azimuth
    /// interpolation.<br />
    /// Examples: f(350, 30, 0.5, 360) = 20, f(100, 80, 0.5, 360) = 90.
    /// </summary>
    public static float ModularLerp(float from, float to, float weight, float modulo) =>
        Mathf.PosMod(from + ModularDelta(from, to, modulo) * weight, modulo);

    /// <summary>
    /// Linear interpolation, but for modular arithmetics with possible wrapping around zero and only moving in positive
    /// direction. Useful for animations.<br />
    /// Examples: f(350, 30, 0.5, 360) = 20, f(100, 80, 0.5, 360) = 270.
    /// </summary>
    public static float ModularLerpPositive(float from, float to, float weight, float modulo) =>
        Mathf.PosMod(from + ModularDeltaPositive(from, to, modulo) * weight, modulo);

    /// <summary>
    /// Find difference, but for modular arithmetics with possible wrapping around zero. Useful for azimuth.<br />
    /// Examples: f(350, 30, 360) = +40, f(100, 80, 360) = -20.
    /// </summary>
    public static float ModularDelta(float from, float to, float modulo)
    {
        if (from < 0 || from >= modulo) throw new ArgumentException();
        if (to < 0 || to >= modulo) throw new ArgumentException();
        
        float delta = to - from;
        if (delta > modulo / 2) delta -= modulo;
        else if (delta < -modulo / 2) delta += modulo;
        return delta;
    }
    
    /// <summary>
    /// Find difference, but for modular arithmetics with possible wrapping around zero and only moving in positive
    /// direction.<br />
    /// Examples: f(350, 30, 360) = +40, f(100, 80, 360) = 340.
    /// </summary>
    public static float ModularDeltaPositive(float from, float to, float modulo)
    {
        if (from < 0 || from >= modulo) throw new ArgumentException();
        if (to < 0 || to >= modulo) throw new ArgumentException();
        
        float delta = to - from;
        if (delta < 0) delta += modulo;
        return delta;
    }
    
    public static T? InterpolateNullable<T>(T? from, T? to, Func<T, T, T> interpolate) where T : struct
    {
        if (from == null || to == null) return to;
        return interpolate(from.Value, to.Value);
    }
    
    public static T? InterpolateNullable<T>(T? from, T? to, Func<T, T, T> interpolate) where T : class
    {
        if (from == null || to == null) return to;
        return interpolate(from, to);
    }
    
    public static double InverseLerp(double from, double to, double value) => (value - from) / (to - from);
    
    public static int PosMod(long value, int modulo) => (int)(value >= 0 ? value % modulo : value % modulo + modulo);
    
    public static float Log(float value, float newBase) => (float)Math.Log(value, newBase);
}