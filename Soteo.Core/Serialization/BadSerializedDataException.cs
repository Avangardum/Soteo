namespace Soteo.Core.Exceptions;

/// <summary>
/// Exception thrown when deserialization fails due to bad input data
/// </summary>
public sealed class BadSerializedDataException(string message, Exception? innerException = null) :
    Exception(message, innerException);
