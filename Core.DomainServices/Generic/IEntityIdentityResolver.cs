using System;
using Infrastructure.Services.Types;

namespace Core.DomainServices.Generic
{
    /// <summary>
    /// Service to resolve an entity uuid based on db or the other way around.
    /// </summary>
    public interface IEntityIdentityResolver
    {
        //TODO: Generics to lazy resolve the repository using dbset?
        Maybe<Guid> ResolveUuid(int dbId);
        Maybe<int> ResolveDbId(Guid uuid);
    }
}
