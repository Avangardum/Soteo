using System.Reflection;

namespace Soteo.Core;

public static class SoteoSharedAssembly
{
    public static readonly Assembly Value = Assembly.GetExecutingAssembly();
}
