using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Statuses;

namespace Soteo.Shared.Extensions;

public static class TempExtensions
{
    extension (Status self)
    {
        public Texture ResolveIcon(PuppetStatusContext context) =>
            self.Icon ?? context.Ability?.Icon ?? MainConst.PlaceholderIcon;
        
        public Texture? Icon =>
            self.IconPath == null ? null : ResourceLoader.Load<Texture>($"res://Textures/Icons/{self.IconPath}.png");
    }
    
    extension (Ability self)
    {
        public Texture Icon => ResourceLoader.Load<Texture>($"res://Textures/Icons/{self.IconPath}.png");
    }
}