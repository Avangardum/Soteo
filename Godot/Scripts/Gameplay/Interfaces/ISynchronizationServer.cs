namespace Soteo.Gameplay.Interfaces;

public interface ISynchronizationServer
{
    void ReceiveSnapshotRequest(Guid clientId);
}
