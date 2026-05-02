namespace Soteo.Gameplay.Interfaces;

public interface IPingMeasurer
{
    double? Ping(Guid peerId);
}