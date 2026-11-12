namespace Soteo.Core.Synchronization;

public interface IShardSynchronizationServer
{
    void ReceiveSnapshotRequest(Guid clientId);
}
