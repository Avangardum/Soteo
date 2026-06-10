namespace Soteo.Core.Interfaces;

public interface ISynchronizationServer
{
    void ReceiveSnapshotRequest(Guid clientId);
}
