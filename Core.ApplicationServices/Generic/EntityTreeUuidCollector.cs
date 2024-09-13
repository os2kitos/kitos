using System;
using System.Collections.Generic;
using Core.DomainModel;

namespace Core.ApplicationServices.Generic
{
    public class EntityTreeUuidCollector: IEntityTreeUuidCollector
    {
        public IEnumerable<Guid?> CollectSelfAndDescendantUuids<T>(T sourceEntity) where T : Entity, IHasUuid, IHierarchy<T>
        {
            var uuids = new List<Guid?>();
            CollectUuidsHelper(sourceEntity);
            return uuids;

            void CollectUuidsHelper(T entity)
            {
                uuids.Add(entity.Uuid);
                foreach (var child in entity.Children)
                {
                    CollectUuidsHelper(child);
                }
            }
        }
    }
}
