using Soteo.Core.Gameplay.Abilities;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Statuses;

namespace Soteo.Shared.Extensions;

public static class IconExtensions
{
    extension (Status self)
    {
        public Texture ResolveIcon(PuppetStatusContext context) =>
            self.Icon ?? context.Ability?.Icon ?? ResourceLoader.Load<Texture>("res://Textures/Icons/Placeholder.png");
        
        public Texture? Icon =>
            self.IconPath == null ? null : ResourceLoader.Load<Texture>($"res://Textures/Icons/{self.IconPath}.png");
    }
    
    extension (Ability self)
    {
        public Texture Icon => ResourceLoader.Load<Texture>($"res://Textures/Icons/{self.IconPath}.png");
    }
}
