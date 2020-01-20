using System;

namespace Infrastructure.Services.Types
{
    public static class TypeHierarchyExtensions
    {
        public static bool IsImplementationOfGenericType(this Type src, Type targetGenericType)
        {
            //Search through the type tree
            while (src != null)
            {
                if (src.IsGenericType && src.GetGenericTypeDefinition() == targetGenericType)
                {
                    return true;
                }

                src = src.BaseType;
            }

            return false;
        }
    }
}
