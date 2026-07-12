namespace Soteo.Util.Extensions;

public static class EnvironmentExtensions
{
    extension (Environment)
    {
        public static string GetRequiredEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name) ??
                throw new Exception($"Environment variable {name} is not defined");
        }
    }
}
