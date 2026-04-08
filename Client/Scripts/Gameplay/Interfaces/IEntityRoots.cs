namespace Soteo.Gameplay.Interfaces;

public interface IEntityRoots
{
    Node2D PlayerCharacterRoot { get; }
    Node2D NonPlayerCharacterRoot { get; }
    Node2D BuildingRoot { get; }
    Node2D ProjectileRoot { get; }
}