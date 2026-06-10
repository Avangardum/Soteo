using Soteo.Util;

namespace Soteo.Main.Gameplay.EntityNodes;

public sealed class AzimuthIndicator : Line2D
{
    private const int SectorCount = 100;
    private const int ArrowHalfWidthSectors = 4;
    private const int EllipseWidth = 8;
    private const int EllipseHeight = 4;
    private const double ArrowTipMultiplier = 1.4;
    private const double LineWidth = 2;
    private const double ZoomFactor = 0.1;
    
    private readonly GdVector2[] _points = new GdVector2[SectorCount + 2 - 2 * (ArrowHalfWidthSectors - 1)];

    public void CalculatePoints(double azimuth, double zoom)
    {
        const double sectorAngle = 2 * Math.PI / SectorCount;
        double forwardAngle = Maths.Deg2Rad(azimuth) - Math.PI / 2;
        var arrowTip = GdVector2.New
        (
            EllipseWidth * Math.Cos(forwardAngle),
            EllipseHeight * Math.Sin(forwardAngle)
        ) * ArrowTipMultiplier;
        _points[0] = arrowTip;
        for (int i = 1; i <= _points.Length - 2; i++)
        {
            double angle = forwardAngle + sectorAngle * (i + ArrowHalfWidthSectors - 1);
            _points[i] = GdVector2.New(EllipseWidth * Math.Cos(angle), EllipseHeight * Math.Sin(angle));
        }
        _points[^2] = _points[0];
        _points[^1] = _points[1];
        
        Points = _points;
        Width = (float)Maths.Lerp(LineWidth / zoom, LineWidth, ZoomFactor);
    }
}
