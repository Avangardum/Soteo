namespace Soteo.Core.Exceptions;

/// <summary>
/// Exception thrown when handling an invalid packet
/// </summary>
public sealed class BadPacketException(string message) : Exception(message);
