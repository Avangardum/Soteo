using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Exceptions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Shared.PacketSerializers;

public abstract class PacketSerializer<TPacket> : IPacketSerializer
    where TPacket : Packet, new()
{
    byte[] IPacketSerializer.Serialize(Packet packet) => Serialize((TPacket)packet);
    
    public byte[] Serialize(TPacket packet)
    {
        var stream = new MemoryStream();
        SerializeInternal(packet, stream);
        return stream.ToArray();
    }
    
    protected virtual void SerializeInternal(TPacket packet, Stream stream)
    {
        SerializeEnum(packet.Type, stream);
    }
    
    Packet IPacketSerializer.Deserialize(Span<byte> bytes) => Deserialize(bytes);
    
    public TPacket Deserialize(Span<byte> bytes)
    {
        try
        {
            var stream = new MemoryStream(bytes.ToArray());
            TPacket packet = DeserializeInternal(stream);
            if (stream.Position != bytes.Length) throw new BadPacketException(
                $"Packet deserialized as {packet}, but contains {bytes.Length - stream.Position} extra bytes");
            return packet;
        }
        catch (BadPacketException e)
        {
            throw new AggregateException($"Bad packet\n{BitConverter.ToString(bytes.ToArray())}\n", e);
        }
    }
    
    protected virtual TPacket DeserializeInternal(Stream stream)
    {
        var packet = new TPacket();
        var type = DeserializeEnum<PacketType>(stream);
        if (type != packet.Type)
            throw new InvalidOperationException("Wrong serializer");
        return packet;
    }
}