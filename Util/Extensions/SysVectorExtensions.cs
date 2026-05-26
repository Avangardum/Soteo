using System.Numerics;

namespace Soteo.Util.Extensions;

public static class SysVectorExtensions
{
    extension (Vector2 self)
    {
        public Vector2 Lerp(Vector2 to, float weight) =>
            new(Maths.Lerp(self.X, to.X, weight), Maths.Lerp(self.Y, to.Y, weight));
        
        public Vector2 Lerp(Vector2 to, double weight) => self.Lerp(to, (float)weight);

        public static Vector2 operator *(Vector2 a, double b) => a * (float)b;
        
        public static Vector2 operator /(Vector2 a, double b) => a / (float)b;
        
        public static Vector2 New(double x, double y) => new((float)x, (float)y);
    }
}
