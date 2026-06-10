namespace Soteo.Core.Gameplay.Interfaces;

public interface IUnitPuppetNode : IEntityNode
{
    bool IsAzimuthIndicatorVisible { get; set; }
    
    bool HalfPixelXVisualOffset { get; }
    bool HalfPixelYVisualOffset { get; }
    
    bool FlipSpriteH { get; set; }
    string Animation { get; set; }
    double AnimationSpeedScale { get; set; }
    int AnimationFrame { get; set; }
    int AnimationFrameCount { get; }
    
    void CalculateAzimuthIndicatorPoints(double azimuth, double zoom);
}
