namespace Soteo.Core.Delegates;

public delegate TElement Deserializer<out TElement>(Stream stream);
