using System.Text;
using Microsoft.Extensions.Configuration;
using FileAccess = Godot.FileAccess;

namespace Soteo.Main.Shared.Extensions;

public static class ConfigurationBuilderExtensions
{
    extension (ConfigurationBuilder self)
    {
        public ConfigurationBuilder AddGodotJsonFile(string path, bool optional)
        {
            if (!FileAccess.FileExists(path))
            {
                if (optional) return self;
                throw new FileNotFoundException($"File {path} not found");
            }
            using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            string text = file.GetAsText();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            var stream = new MemoryStream(bytes);
            self.AddJsonStream(stream);
            return self;
        }
    }
}
