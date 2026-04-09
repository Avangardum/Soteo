namespace Soteo.Shared.Extensions;

public static class VectorExtensions
{
    extension (Vector2 self)
    {
        public Vector2 Lerp(Vector2 to, float weight) =>
            new(Mathf.Lerp(self.x, to.x, weight), Mathf.Lerp(self.y, to.y, weight));
    }
}