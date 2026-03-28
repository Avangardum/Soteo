using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;

namespace Soteo.MasterServer.Interfaces;

public interface IWebSocketRepository
{
    bool TryGet(Guid userId, [NotNullWhen(true)] out WebSocket? ws);

    /// <summary>
    /// Closes the WebSocket currently associated with this user, waits for the call to Remove, then adds the given
    /// WebSocket as the new one. This method must be awaited before processing user connection because it waits for
    /// disconnection logic to finish and the old WebSocket to be removed afterward. This protects from race conditions
    /// which may arise from concurrent connection / disconnection.
    /// </summary>
    Task CloseOldAndSetAsync(Guid userId, WebSocket ws);

    /// <summary>
    /// Remove the WebSocket associated with the given user. This method should be called after the app fully processed
    /// user disconnection because a new connection may proceed after it's called. This protects from race conditions
    /// which may arise from concurrent connection / disconnection.
    /// </summary>
    void Remove(Guid userId);
}