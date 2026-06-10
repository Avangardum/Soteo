namespace Soteo.Core.Exceptions;

public sealed class BadPacketException(string reason) : Exception(reason)
{
    public string Reason => reason;
}
