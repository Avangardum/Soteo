namespace Soteo.Core.Serialization;

public delegate TElement Deserializer<out TElement>(Stream stream);
