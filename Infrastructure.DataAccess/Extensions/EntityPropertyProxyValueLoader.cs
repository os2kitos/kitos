using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infrastructure.DataAccess.Extensions
{
    public class EntityPropertyProxyValueLoader<T>
    {
        private readonly Lazy<IReadOnlyList<MethodInfo>> _entityTypeVirtualGetters =
            new(
                () =>
                    typeof(T)
                        .GetProperties()
                        .Where(property => property.CanRead)
                        .Select(property => property.GetMethod)
                        .Where(getMethod => getMethod.IsVirtual && getMethod.IsPublic)
                        .ToList()
                        .AsReadOnly()
            );

        public T LoadReferencedEntities(T entity)
        {
            //Invoke getters of all reference properties. This solves the issue of EF not cascading on optional foreign keys in reference objects which may reference either one or many of the different root elements. Example: TaskRef
            _entityTypeVirtualGetters
                .Value
                .Select(methodInfo => methodInfo.Invoke(entity, new object[0]))
                .ToList();
            return T;
        }
    }
}
