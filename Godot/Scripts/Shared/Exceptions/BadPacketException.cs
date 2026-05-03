namespace Soteo.Shared.Exceptions;

public sealed class BadPacketException(string reason) : Exception(reason)
{
    public string Reason => reason;
}