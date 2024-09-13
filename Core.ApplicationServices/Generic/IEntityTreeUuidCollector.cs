using System;
using System.Collections.Generic;
using Core.DomainModel;

namespace Core.ApplicationServices.Generic
{
    public interface IEntityTreeUuidCollector
    {
        IEnumerable<Guid?> CollectSelfAndDescendantUuids<T>(T sourceEntity) where T: Entity, IHasUuid, IHierarchy<T>;
    }
}
