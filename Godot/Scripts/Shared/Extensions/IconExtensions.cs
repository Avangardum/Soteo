using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Items;
using Soteo.Core.Statuses;

namespace Soteo.Main.Shared.Extensions;

public static class IconExtensions
{
    private static readonly Texture2D Placeholder = ResourceLoader.Load<Texture2D>("res://Textures/Icons/Placeholder.png");

    extension (Status self)
    {
        public Texture2D ResolveIcon(PuppetStatusContext context) =>
            self.Icon ?? context.Ability?.Icon ?? Placeholder;

        public Texture2D? Icon
        {
            get
            {
                return self.IconPath == null ?
                    null :
                    ResourceLoader.Load<Texture2D>($"res://Textures/Icons/{self.IconPath}.png") ?? Placeholder;
            }
        }
    }

    extension (Ability self)
    {
        public Texture2D Icon => ResourceLoader.Load<Texture2D>($"res://Textures/Icons/{self.IconPath}.png") ?? Placeholder;
    }

    extension (Item self)
    {
        public Texture2D Icon => ResourceLoader.Load<Texture2D>($"res://Textures/Icons/{self.IconPath}.png") ?? Placeholder;
    }
}
