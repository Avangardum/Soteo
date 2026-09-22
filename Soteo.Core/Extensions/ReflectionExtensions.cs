using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Extensions;

public static class ReflectionExtensions
{
    extension (Type self)
    {
        /// <summary>
        /// For a type inheriting from a base generic type using the given definition with a single type argument,
        /// get the argument. Null if not inherited from a generic type using the given definition.
        /// Example: typeof(List<int>).SingleTypeArgOfGenericDefinitionOrNull(typeof(List<>)) returns typeof(int).
        /// </summary>
        public Type? SingleTypeArgOfGenericDefinitionOrNull(Type baseGenericClassDefinition)
        {
            if (!baseGenericClassDefinition.IsGenericTypeDefinition)
                throw new ArgumentException($"{baseGenericClassDefinition} is not a generic class definition");
            if (baseGenericClassDefinition.GetGenericArguments().Length != 1)
                throw new ArgumentException($"{baseGenericClassDefinition} doesn't have exactly 1 generic argument");
            Type? baseGenericClass = self.BaseTypes
                .SingleOrDefault
                (
                    bt => bt.IsConstructedGenericType && bt.GetGenericTypeDefinition() == baseGenericClassDefinition
                );
            if (baseGenericClass == null) return null;
            return baseGenericClass.GenericTypeArguments.Single();
        }
    }
}
