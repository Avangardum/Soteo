namespace Soteo.Core.Gameplay.Interfaces;

public interface ISynchronizationServer
{
    void ReceiveSnapshotRequest(Guid clientId);
}
