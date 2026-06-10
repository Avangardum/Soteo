namespace Soteo.Core.Gameplay.Interfaces;

public interface INetworkDebugger
{
    long BytesSent { get; }
    long BytesReceived { get; }
    double? Ping(Guid peerId);
}
