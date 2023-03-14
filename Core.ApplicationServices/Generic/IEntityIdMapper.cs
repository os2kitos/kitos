using Core.Abstractions.Types;
using Core.DomainModel;
using System;

namespace Core.ApplicationServices.Generic
{
    public interface IEntityIdMapper
    {
        Result<int, OperationError> Map<TEntity>(Guid uuid) where TEntity : Entity, IHasUuid;
        Result<Guid, OperationError> Map<TEntity>(int id) where TEntity : Entity, IHasUuid;
    }
}
