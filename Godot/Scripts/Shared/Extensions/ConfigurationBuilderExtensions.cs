using System.Text;
using Microsoft.Extensions.Configuration;

namespace Soteo.Main.Shared.Extensions;

public static class ConfigurationBuilderExtensions
{
    extension (ConfigurationBuilder self)
    {
        public ConfigurationBuilder AddGodotJsonFile(string path, bool optional)
        {
            var file = new GdFile();
            if (!file.FileExists(path))
            {
                if (optional) return self;
                throw new FileNotFoundException($"File {path} not found");
            }
            file.Open(path, GdFile.ModeFlags.Read);
            string text = file.GetAsText();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            var stream = new MemoryStream(bytes);
            self.AddJsonStream(stream);
            return self;
        }
    }
}
