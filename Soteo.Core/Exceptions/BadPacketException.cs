namespace Soteo.Core.Shared.Exceptions;

public sealed class BadPacketException(string reason) : Exception(reason)
{
    public string Reason => reason;
}
