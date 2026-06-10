using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Statuses;

namespace Soteo.Main.Shared.Extensions;

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
