namespace Soteo.Core.Serialization;

[AttributeUsage(AttributeTargets.Class)]
public abstract class PacketSerializerAttribute : Attribute
{
    public abstract Type SerializerType { get; }
}

public sealed class PacketSerializerAttribute<T> : PacketSerializerAttribute where T : IPacketSerializer
{
    public override Type SerializerType => typeof(T);
}
