namespace Soteo.Gameplay.Interfaces;

public interface IPingMeasurer
{
    float? Ping(Guid peerId);
}