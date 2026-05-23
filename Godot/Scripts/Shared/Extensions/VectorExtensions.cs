namespace Soteo.Shared.Extensions;

public static class VectorExtensions
{
    extension (GdVector2 self)
    {
        public GdVector2 Lerp(GdVector2 to, float weight) =>
            new(Mathf.Lerp(self.x, to.x, weight), Mathf.Lerp(self.y, to.y, weight));
        
        public GdVector2 Lerp(GdVector2 to, double weight) => self.Lerp(to, (float)weight);

        public static GdVector2 operator *(GdVector2 a, double b) => a * (float)b;
        
        public static GdVector2 operator /(GdVector2 a, double b) => a / (float)b;
        
        public static GdVector2 New(double x, double y) => new((float)x, (float)y);
    }
}