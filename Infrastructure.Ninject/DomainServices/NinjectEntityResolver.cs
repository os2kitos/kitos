using System;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
using Ninject;

namespace Infrastructure.Ninject.DomainServices
{
    /// <summary>
    /// Ninject adapter for the <see cref="IEntityResolver"/> domain service.
    /// This implementation ensures that we can quickly exchange uuid for full entity without needing a reference to all repositories in the client context.
    /// ONLY for use where access control has been established!!
    /// </summary>
    public class NinjectEntityResolver : IEntityResolver
    {
        private readonly IKernel _kernel;

        public NinjectEntityResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public Result<T, OperationError> ResolveEntityFromUuid<T>(Guid uuid) where T : class, IHasUuid
        {
            return _kernel
                .Get<IGenericRepository<T>>()
                .AsQueryable()
                .ByUuid(uuid)
                .FromNullable()
                .Match<Result<T, OperationError>>(t => t, () => new OperationError($"Failed to resolve {typeof(T).Name} with Uuid: {uuid}", OperationFailure.BadInput));
        }
    }
}
