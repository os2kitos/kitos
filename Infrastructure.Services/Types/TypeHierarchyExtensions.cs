using System;

namespace Infrastructure.Services.Types
{
    public static class TypeHierarchyExtensions
    {
        public static bool IsImplementationOfGenericType(this Type src, Type targetGenericType)
        {
            //Search through the type tree
            Type? current = src;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == targetGenericType)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }
    }
}
