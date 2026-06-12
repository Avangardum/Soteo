namespace Soteo.Core.Delegates;

public delegate void Serializer<in TElement>(TElement value, Stream stream);
