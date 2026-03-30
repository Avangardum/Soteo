using System.Collections.Concurrent;
using System.Net.WebSockets;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets.Shared;

namespace Soteo.MasterServer;

public class WebSocketPacketSender(IWebSocketRepository wsRepo, IPacketSerializer packetSerializer) : IPacketSender
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _mutexesByUserId = [];
    
    public async Task SendToAsync(Packet packet, Guid receiverId)
    {
        if (!wsRepo.TryGet(receiverId, out WebSocket? ws)) return;
        byte[] bytes = packetSerializer.Serialize(packet);
        SemaphoreSlim mutex = _mutexesByUserId.GetOrAdd(receiverId, _ => new SemaphoreSlim(1));
        await mutex.WaitAsync();
        _ = ws.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);
        mutex.Release();
    }
}