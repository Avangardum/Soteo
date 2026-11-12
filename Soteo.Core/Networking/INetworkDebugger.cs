namespace Soteo.Core.Networking;

public interface INetworkDebugger
{
    long BytesSent { get; }
    long BytesReceived { get; }
    double? Ping(Guid peerId);
}
