using Core.DomainModel;
using Core.DomainServices.Extensions;

namespace Core.DomainServices.Queries
{
    /// <summary>
    /// Migration class used to support generic queries to be executed in the database
    /// Use this class when type constraints in "too generic" contexts prevent use of the <see cref="IDomainQuery{T}"/> or <see cref="EntityQueryExtensions"/> types directly
    /// </summary>
    public static class QueryFactory
    {
        public static IDomainQuery<T> ByOrganizationId<T>(int organizationId)
        {
            var constructor =
                typeof(QueryByOrganizationId<>)
                    .MakeGenericType(typeof(T))
                    .GetConstructor(new[] { typeof(int) });
            return (IDomainQuery<T>)constructor?.Invoke(new object[] { organizationId });
        }

        public static IDomainQuery<T> ByPublicAccessModifier<T>()
        {
            var constructor =
                typeof(QueryByAccessModifier<>)
                    .MakeGenericType(typeof(T))
                    .GetConstructor(new[] { typeof(AccessModifier) });
            return (IDomainQuery<T>)constructor?.Invoke(new object[] { AccessModifier.Public });
        }

        public static IDomainQuery<T> ByPublicAccessOrOrganizationId<T>(int organizationId)
        {
            var constructor =
                typeof(QueryByPublicAccessOrOrganizationId<>)
                    .MakeGenericType(typeof(T))
                    .GetConstructor(new[] { typeof(int) });
            return (IDomainQuery<T>)constructor?.Invoke(new object[] { organizationId });
        }
    }
}
