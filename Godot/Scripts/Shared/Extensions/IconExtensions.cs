using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Items;
using Soteo.Core.Statuses;

namespace Soteo.Main.Shared.Extensions;

public static class IconExtensions
{
    private static readonly Texture Placeholder = ResourceLoader.Load<Texture>("res://Textures/Icons/Placeholder.png");

    extension (Status self)
    {
        public Texture ResolveIcon(PuppetStatusContext context) =>
            self.Icon ?? context.Ability?.Icon ?? Placeholder;

        public Texture? Icon
        {
            get
            {
                return self.IconPath == null ?
                    null :
                    ResourceLoader.Load<Texture>($"res://Textures/Icons/{self.IconPath}.png") ?? Placeholder;
            }
        }
    }

    extension (Ability self)
    {
        public Texture Icon => ResourceLoader.Load<Texture>($"res://Textures/Icons/{self.IconPath}.png") ?? Placeholder;
    }

    extension (Item self)
    {
        public Texture Icon => ResourceLoader.Load<Texture>($"res://Textures/Icons/{self.IconPath}.png") ?? Placeholder;
    }
}
