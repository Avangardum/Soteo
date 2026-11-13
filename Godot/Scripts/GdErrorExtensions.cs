namespace Soteo.Main;

public static class GdErrorExtensions
{
    extension (Error self)
    {
        public void ShouldBeOk()
        {
            if (self != Error.Ok)
                throw new Exception($"Unexpected Godot error: {self}");
        }
    }
}
