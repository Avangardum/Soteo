namespace Soteo.Core.Interfaces;

public interface IShardSynchronizationServer
{
    void ReceiveSnapshotRequest(Guid clientId);
}
