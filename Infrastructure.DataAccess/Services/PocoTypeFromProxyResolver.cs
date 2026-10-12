using System;
using Infrastructure.Services.DataAccess;

namespace Infrastructure.DataAccess.Services
{
    /// <summary>
    /// Resolves EF Core proxy types (created by UseLazyLoadingProxies via Castle DynamicProxy)
    /// back to their underlying POCO types. Proxy types live in the "Castle.Proxies" namespace
    /// and inherit from the real entity class.
    /// </summary>
    public class PocoTypeFromProxyResolver : IEntityTypeResolver
    {
        public Type Resolve(Type entityType)
        {
            // Castle DynamicProxy (used by EF Core lazy loading) places generated proxy types
            // in the "Castle.Proxies" namespace. Walk up the hierarchy to find the real type.
            var type = entityType;
            while (type?.Namespace == "Castle.Proxies")
            {
                type = type.BaseType;
            }
            return type ?? entityType;
        }
    }
}
