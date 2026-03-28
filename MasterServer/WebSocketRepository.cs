using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using Soteo.MasterServer.Interfaces;

namespace Soteo.MasterServer;

public sealed class WebSocketRepository : IWebSocketRepository
{
    private readonly ConcurrentDictionary<Guid, (WebSocket Ws, TaskCompletionSource Removed)> _webSocketsByUserId = [];
    
    public bool TryGet(Guid userId, [NotNullWhen(true)] out WebSocket? ws)
    {
        bool found = _webSocketsByUserId.TryGetValue(userId, out var tuple);
        ws = tuple.Ws;
        return found;
    }

    /// <inheritdoc/>
    public async Task CloseOldAndSetAsync(Guid userId, WebSocket ws)
    {
        while (!_webSocketsByUserId.TryAdd(userId, (ws, new TaskCompletionSource())))
        {
            if (!_webSocketsByUserId.TryGetValue(userId, out var tuple)) continue;
            await tuple.Ws.CloseAsync
            (
                WebSocketCloseStatus.PolicyViolation,
                "Another connection was opened.",
                CancellationToken.None
            );
            await tuple.Removed.Task;
        }
    }
    
    /// <inheritdoc/>
    public void Remove(Guid userId)
    {
        _webSocketsByUserId.Remove(userId, out var tuple);
        tuple.Removed.SetResult();
    }
}