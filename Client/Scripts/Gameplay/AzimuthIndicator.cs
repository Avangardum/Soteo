using Soteo.Shared;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay;

public sealed class AzimuthIndicator : Line2D
{
    private const int SectorCount = 100;
    private const int ArrowHalfWidthSectors = 4;
    private const int EllipseWidth = 8;
    private const int EllipseHeight = 4;
    private const double ArrowTipMultiplier = 1.4;
    
    private readonly Vector2[] _points;
    
    public AzimuthIndicator()
    {
        _points = new Vector2[SectorCount + 2 - 2 * (ArrowHalfWidthSectors - 1)];
        CalculatePoints();
    }
    
    public double Azimuth
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
        
        const double sectorAngle = 2 * Math.PI / SectorCount;
        double forwardAngle = Maths.Deg2Rad(Azimuth) - Math.PI / 2;
        var arrowTip = Vector2.New
        (
            EllipseWidth * Math.Cos(forwardAngle),
            EllipseHeight * Math.Sin(forwardAngle)
        ) * ArrowTipMultiplier;
        _points[0] = arrowTip;
        for (int i = 1; i <= _points.Length - 2; i++)
        {
            double angle = forwardAngle + sectorAngle * (i + ArrowHalfWidthSectors - 1);
            _points[i] = Vector2.New(EllipseWidth * Math.Cos(angle), EllipseHeight * Math.Sin(angle));
        }
        _points[^2] = _points[0];
        _points[^1] = _points[1];
        
        Points = _points;
    }
}