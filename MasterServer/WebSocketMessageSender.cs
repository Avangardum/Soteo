using System.Collections.Concurrent;
using System.Net.WebSockets;
using Soteo.MasterServer.Interfaces;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Messages.Shared;

namespace Soteo.MasterServer;

public class WebSocketMessageSender(IWebSocketRepository wsRepo, IMessageSerializer messageSerializer) : IMessageSender
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _mutexesByUserId = [];
    
    public async Task SendToAsync(Message message, Guid receiverId)
    {
        if (!wsRepo.TryGet(receiverId, out WebSocket? ws)) return;
        byte[] bytes = messageSerializer.Serialize(message);
        SemaphoreSlim mutex = _mutexesByUserId.GetOrAdd(receiverId, _ => new SemaphoreSlim(1));
        await mutex.WaitAsync();
        _ = ws.SendAsync(bytes, WebSocketMessageType.Binary, true, CancellationToken.None);
        mutex.Release();
    }
}