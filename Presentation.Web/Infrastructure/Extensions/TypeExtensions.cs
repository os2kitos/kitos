using System;

namespace Presentation.Web.Infrastructure.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNullableEnum(this Type src)
        {
            return
                src != null &&
                src.IsGenericType &&
                src.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                src.GetGenericArguments()[0].IsEnum;
        }
    }
}