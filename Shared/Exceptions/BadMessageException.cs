namespace Soteo.Shared.Exceptions;

public sealed class BadMessageException(string reason) : Exception(reason)
{
    public string Reason => reason;
}