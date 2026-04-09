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
    /// interpolation. Examples: f(350, 30, 0.5, 360) = 20, f(100, 80, 0.5, 360) = 90.
    /// </summary>
    public static float ModularLerp(float from, float to, float weight, float modulo)
    {
        if (from < 0 || from >= modulo) throw new ArgumentException();
        if (to < 0 || to >= modulo) throw new ArgumentException();
        
        float fullDelta = to - from;
        if (fullDelta > modulo / 2) fullDelta -= modulo;
        else if (fullDelta < -modulo / 2) fullDelta += modulo;
        
        return Mathf.PosMod(from + fullDelta * weight, modulo);
    }
    
    /// <summary>
    /// Linear interpolation, but for modular arithmetics with possible wrapping around zero and only moving in positive
    /// direction. Useful for animations. Examples: f(350, 30, 0.5, 360) = 20, f(100, 80, 0.5, 360) = 270.
    /// </summary>
    public static float ModularLerpPositive(float from, float to, float weight, float modulo)
    {
        if (from < 0 || from >= modulo) throw new ArgumentException();
        if (to < 0 || to >= modulo) throw new ArgumentException();
        
        float fullDelta = to - from;
        if (fullDelta < 0) fullDelta += modulo;
        
        return Mathf.PosMod(from + fullDelta * weight, modulo);
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
}