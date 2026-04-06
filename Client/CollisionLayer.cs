namespace Soteo.Client;

[Flags]
public enum CollisionLayer : uint
{
    None = 0u,
    All = ~0u,
    Default = 1u,
    ClickArea = 1u << 8
}