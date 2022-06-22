using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;

namespace Core.ApplicationServices.Helpers
{
    public static class NamedEntityMappingHelper
    {
        public static NamedEntity ToNamedEntity<T>(this T entity) where T : IHasId, IHasName
        {
            return new NamedEntity(entity.Id, entity.Name);
        }
    }
}
