namespace Soteo.Gameplay;

public sealed class AzimuthIndicator : Line2D
{
    private const int SectorCount = 100;
    private const int ArrowHalfWidthSectors = 4;
    private const int EllipseWidth = 8;
    private const int EllipseHeight = 4;
    private const float ArrowTipMultiplier = 1.4f;
    
    private readonly Vector2[] _points;
    
    public AzimuthIndicator()
    {
        _points = new Vector2[SectorCount + 2 - 2 * (ArrowHalfWidthSectors - 1)];
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
        if (IsServer) return;
        
        const float sectorAngle = 2 * Mathf.Pi / SectorCount;
        float forwardAngle = Mathf.Deg2Rad(Azimuth) - Mathf.Pi / 2;
        var arrowTip = new Vector2
        (
            EllipseWidth * Mathf.Cos(forwardAngle),
            EllipseHeight * Mathf.Sin(forwardAngle)
        ) * ArrowTipMultiplier;
        _points[0] = arrowTip;
        for (int i = 1; i <= _points.Length - 2; i++)
        {
            float angle = forwardAngle + sectorAngle * (i + ArrowHalfWidthSectors - 1);
            _points[i] = new Vector2(EllipseWidth * Mathf.Cos(angle), EllipseHeight * Mathf.Sin(angle));
        }
        _points[^2] = _points[0];
        _points[^1] = _points[1];
        
        Points = _points;
    }
}