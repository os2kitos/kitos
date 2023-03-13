using Core.Abstractions.Types;
using Core.DomainModel;
using System;

namespace Core.ApplicationServices.Generic
{
    public interface IExtendedEntityIdentityResolverService
    {
        Result<int, OperationError> ExchangeId<TEntity>(Guid uuid) where TEntity : Entity, IHasUuid;
        Result<Guid, OperationError> ExchangeId<TEntity>(int id) where TEntity : Entity, IHasUuid;
    }
}
