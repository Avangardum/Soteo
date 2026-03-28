using System.Collections.Concurrent;
using System.Net.WebSockets;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Messages.Shared;

namespace Soteo.MasterServer;

public class WebSocketMessageSender(IWebSocketRepository wsRepo) : IMessageSender
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _mutexesByUserId = [];
    
    public async Task SendToAsync(Message message, Guid receiverId)
    {
        if (!wsRepo.TryGet(receiverId, out WebSocket? ws)) return;
        byte[] bytes = TypeLocator.MessageSerializers[message.Type].Serialize(message);
        SemaphoreSlim mutex = _mutexesByUserId.GetOrAdd(receiverId, _ => new SemaphoreSlim(1));
        await mutex.WaitAsync();
        _ = ws.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);
        mutex.Release();
    }
}