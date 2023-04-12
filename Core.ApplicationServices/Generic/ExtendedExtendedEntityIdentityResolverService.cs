using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices.Generic;
using System;

namespace Core.ApplicationServices.Generic
{
    public class EntityIdMapper : IEntityIdMapper
    {
        private readonly IEntityIdentityResolver _entityIdentityResolver;

        public EntityIdMapper(IEntityIdentityResolver entityIdentityResolver)
        {
            _entityIdentityResolver = entityIdentityResolver;
        }

        public Result<int, OperationError> Map<TEntity>(Guid uuid) where TEntity : Entity, IHasUuid
        {
            return _entityIdentityResolver.ResolveDbId<TEntity>(uuid)
                .Match<Result<int, OperationError>>(id => id, () => new OperationError($"{typeof(TEntity).Name} with uuid: {uuid} was not found", OperationFailure.NotFound));
        }

        public Result<Guid, OperationError> Map<TEntity>(int id) where TEntity : Entity, IHasUuid
        {
            return _entityIdentityResolver.ResolveUuid<TEntity>(id)
                .Match<Result<Guid, OperationError>>(uuid => uuid, () => new OperationError($"{typeof(TEntity).Name} with id: {id} was not found", OperationFailure.NotFound));
        }
    }
}
