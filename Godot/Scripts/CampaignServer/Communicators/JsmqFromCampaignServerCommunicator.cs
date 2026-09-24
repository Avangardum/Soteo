using System.Diagnostics.CodeAnalysis;
using Soteo.Core;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Main.CampaignServer.Communicators;

/// <summary>
/// Communicator using the JavaScript message queue instead of WebSockets. Used for singleplayer in browser.
/// </summary>
public sealed class JsmqFromCampaignServerCommunicator
(
    IPacketSerializer packetSerializer,
    IPacketHandler packetHandler,
    IUserRepository userRepo,
    IInitializationRepository initRepo
) : GdObject, IFromCampaignServerCommunicator
{
    private readonly HashSet<Guid> _peerIds = [];

    public event Action<Guid> PeerConnected = delegate {};
    public event Action<Guid> PeerDisconnected = delegate {};

    public void Poll()
    {
        while (true)
        {
            if (!TryReceivePacket(out Packet? packet, out Guid senderId)) return;
            if (!_peerIds.Contains(senderId))
                HandleHandshakePacket(packet, senderId);
            else
                packetHandler.HandleAsync(packet, senderId).CollectException();
        }
    }

    private bool TryReceivePacket([NotNullWhen(true)] out Packet? packet, out Guid senderId)
    {
        var base64 = (string?)JavaScript.Eval($"""jsmq.receive("{Const.CampaignServerId}")""");
        if (base64 == null)
        {
            packet = null;
            senderId = Guid.Empty;
            return false;
        }
        byte[] bytes = Convert.FromBase64String(base64);
        senderId = new Guid(bytes.AsSpan()[..Const.BytesInGuid].ToArray());
        packet = packetSerializer.Deserialize(bytes.AsSpan()[Const.BytesInGuid..]);
        return true;
    }

    private void HandleHandshakePacket(Packet packet, Guid senderId)
    {
        if (packet is not CampaignServerHandshakePacket handshake)
            throw new Exception("Handshake packet expected");
        var claims = new Dictionary<string, object>
        {
            ["sub"] = senderId.ToString(),
            // When using JSMQ, role is sent instead of token
            [handshake.Token] = true
        };
        bool isPlayer = claims.TryGetValue("player", out object value) && value is true;
        if (isPlayer && !initRepo.IsInitialized)
        {
            var reason = "Not accepting player connections yet, try again later";
            SendTo(new BadInputPacket { Reason = reason }, senderId);
            return;
        } // todo this crashes the client, make it a popup instead
        userRepo.OnConnected(claims);
        _peerIds.Add(senderId);
        PeerConnected(senderId);
        SendTo(new CampaignServerHandshakeAckPacket(), senderId);
    }

    public void SendTo(Packet packet, params IEnumerable<Guid> receiverIds)
    {
        string base64 = ToBase64(packet);
        foreach (Guid id in receiverIds)
            JavaScript.Eval($"""jsmq.send("{base64}", "{id}");""");
    }

    public void BroadcastToAll(Packet packet)
    {
        string base64 = ToBase64(packet);
        foreach (Guid id in _peerIds)
            JavaScript.Eval($"""jsmq.send("{base64}", "{id}");""");
    }

    public void BroadcastToShardServers(Packet packet)
    {
        string base64 = ToBase64(packet);
        foreach (Guid id in userRepo.Values.Where(it => it.IsShard).Select(it => it.Id))
            JavaScript.Eval($"""jsmq.send("{base64}", "{id}");""");
    }

    public void BroadcastToClients(Packet packet)
    {
        string base64 = ToBase64(packet);
        foreach (Guid id in userRepo.Values.Where(it => it.IsPlayer).Select(it => it.Id))
            JavaScript.Eval($"""jsmq.send("{base64}", "{id}");""");
    }

    private string ToBase64(Packet packet)
    {
        byte[] bytes = [..Const.CampaignServerId.ToByteArray(), ..packetSerializer.Serialize(packet)];
        return Convert.ToBase64String(bytes);
    }

    public void RelayFrom(RelayedPacket packet, Guid senderId)
    {
        SendTo(packet with { PeerId = senderId }, packet.PeerId);
    }
}
