namespace Soteo.Client.Extensions;

public static class VectorExtensions
{
    extension (GdVector2 self)
    {
        public SysVector2 Sys => new(self.x, self.y);
    }
    
    extension (SysVector2 self)
    {
        public GdVector2 Gd => new(self.X, self.Y);
    }
}