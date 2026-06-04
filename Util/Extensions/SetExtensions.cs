using Soteo.Util.Interfaces;

namespace Soteo.Util.Extensions;

public static class SetExtensions
{
    extension<T> (ISet<T> self)
    {
        public IReadOnlySet<T> AsReadOnly() => new ReadOnlySetWrapper<T>(self);
    }
}
