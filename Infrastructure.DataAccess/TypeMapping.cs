using System.Data.Entity.ModelConfiguration.Configuration;
using Core.DomainModel;
using Infrastructure.DataAccess.Mapping;

namespace Infrastructure.DataAccess
{
    public static class TypeMapping
    {
        /// <summary>
        /// Creates a non-clustered index for access modifier
        /// </summary>
        /// <param name="map">Source entity mapping</param>
        /// <returns></returns>
        public static PrimitivePropertyConfiguration AddIndexOnAccessModifier<TMap, TType>(TMap map)
            where TType : Entity, IHasAccessModifier
            where TMap : EntityMap<TType>
        {
            return map
                .Property(x => x.AccessModifier)
                .HasIndexAnnotation("UX_AccessModifier", 0);
        }
    }
}
