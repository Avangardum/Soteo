namespace Soteo.Main.Shared.Extensions;

public static class GdErrorExtensions
{
    extension (Error self)
    {
        public void ThrowIfError()
        {
            if (self != Error.Ok)
                throw new Exception($"Unexpected Godot error: {self}");
        }
    }
}
