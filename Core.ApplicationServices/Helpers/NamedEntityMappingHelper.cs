using Core.ApplicationServices.Model.Shared;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;

namespace Core.ApplicationServices.Helpers
{
    public static class NamedEntityMappingHelper
    {
        public static NamedEntity ToNamedEntity<T>(this T entity) where T : IHasId, IHasName
        {
            return new NamedEntity(entity.Id, entity.Name);
        }
        public static NamedEntityWithUuid ToNamedEntityWithUuid<T>(this T entity) where T : IHasId, IHasUuid, IHasName
        {
            return new NamedEntityWithUuid(entity.Id, entity.Name, entity.Uuid);
        }
        public static NamedEntityWithUuid ToNamedEntityWithUuid(this ItSystemUsage entity)
        {
            return new NamedEntityWithUuid(entity.Id, entity.ItSystem.Name, entity.Uuid);
        }
    }
}
