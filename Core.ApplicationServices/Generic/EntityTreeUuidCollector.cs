using System;
using System.Collections.Generic;
using Core.DomainModel;

namespace Core.ApplicationServices.Generic
{
    public class EntityTreeUuidCollector: IEntityTreeUuidCollector
    {
        public IEnumerable<Guid?> Collect<T>(T sourceEntity) where T : Entity, IHasUuid, IHierarchy<T>
        {
            var uuids = new List<Guid?>();
            CollectChildUuidsHelper(sourceEntity);
            return uuids;

            void CollectChildUuidsHelper(T entity)
            {
                uuids.Add(entity.Uuid);
                foreach (var child in entity.Children)
                {
                    CollectChildUuidsHelper(child);
                }
            }
        }
    }
}
