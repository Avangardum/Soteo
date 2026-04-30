namespace Soteo.Gameplay;

public sealed class AzimuthIndicator : Line2D
{
    public AzimuthIndicator()
    {
        CalculatePoints();
    }
    
    public float Azimuth
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            CalculatePoints();
        }
    }
    
    private void CalculatePoints()
    {
        const int sectorCount = 10;
        const int arrowHalfWidthSectors = 1;
        const int width = 10;
        const int height = 5;
        const float arrowTipMultiplier = 1.2f;
        
        const float sectorAngle = 2 * Mathf.Pi / sectorCount;
        var points = new Vector2[sectorCount + 1 - 2 * (arrowHalfWidthSectors - 1)];
        float azimuthRad = Mathf.Deg2Rad(Azimuth);
        var arrowTip = new Vector2(width * Mathf.Cos(azimuthRad), height * Mathf.Sin(azimuthRad)) * arrowTipMultiplier;
        points[0] = arrowTip;
        points[^1] = arrowTip;
        for (int i = 1; i <= points.Length - 2; i++)
        {
            float angle = azimuthRad + sectorAngle * (i + arrowHalfWidthSectors - 1);
            points[i] = new Vector2(width * Mathf.Cos(angle), height * Mathf.Sin(angle));
        }
        
        Points = points;
    }
}