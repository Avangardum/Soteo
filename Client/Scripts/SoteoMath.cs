namespace Soteo.Client;

public static class SoteoMath
{
    public static float DirectionToAzimuth(Vector2 direction) =>
        Mathf.Rad2Deg(Mathf.PosMod(Mathf.Atan2(direction.y, direction.x) + Mathf.Pi / 2, 2 * Mathf.Pi));
    
    public static Vector2 AzimuthToDirection(float azimuth)
    {
        float atan2 = Mathf.Deg2Rad(azimuth) - Mathf.Pi / 2;
        return new Vector2(Mathf.Cos(atan2), Mathf.Sin(atan2));
    }
}