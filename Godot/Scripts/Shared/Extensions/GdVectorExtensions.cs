using Soteo.Util;

namespace Soteo.Shared.Extensions;

public static class GdVectorExtensions
{
    extension (GdVector2 self)
    {
        public Vector2 ToSys() => new(self.x, self.y);
        
        public GdVector2 Lerp(GdVector2 to, float weight) =>
            new(Maths.Lerp(self.x, to.x, weight), Maths.Lerp(self.y, to.y, weight));
        
        public GdVector2 Lerp(GdVector2 to, double weight) => self.Lerp(to, (float)weight);

        public static GdVector2 operator *(GdVector2 a, double b) => a * (float)b;
        
        public static GdVector2 operator /(GdVector2 a, double b) => a / (float)b;
        
        public static GdVector2 New(double x, double y) => new((float)x, (float)y);
    }
    
    extension (Vector2 self)
    {
        public GdVector2 ToGd() => new(self.X, self.Y);
    }
}
