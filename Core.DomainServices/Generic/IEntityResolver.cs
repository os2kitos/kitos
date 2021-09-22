using System;
using Core.Abstractions.Types;
using Core.DomainModel;

namespace Core.DomainServices.Generic
{
    /// <summary>
    /// Service to resolve an entity by uuid.
    /// </summary>
    public interface IEntityResolver
    {
        Result<T, OperationError> ResolveEntityFromUuid<T>(Guid uuid) where T : class, IHasUuid;
    }
}
