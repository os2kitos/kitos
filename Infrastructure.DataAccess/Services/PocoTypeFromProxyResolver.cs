using System;
using System.Data.Entity.Core.Objects;
using Infrastructure.Services.DataAccess;

namespace Infrastructure.DataAccess.Services
{
    /// <summary>
    /// Resolves the POCO type for a EF created proxy type.
    /// </summary>
    public class PocoTypeFromProxyResolver : IEntityTypeResolver
    {
        public Type Resolve(Type entityType)
        {
            return ObjectContext.GetObjectType(entityType);
        }
    }
}
