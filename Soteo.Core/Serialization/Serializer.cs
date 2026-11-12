namespace Soteo.Core.Serialization;

public delegate void Serializer<in TElement>(TElement value, Stream stream);
