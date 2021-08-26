using System;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
using Infrastructure.Services.Types;
using Ninject;

namespace Infrastructure.Ninject.DomainServices
{
    /// <summary>
    /// Ninject adapter for the <see cref="IEntityIdentityResolver"/> domain service.
    /// This implementation ensures that we can quickly exchange id for uuid without needing a reference to all repositories in the client context.
    /// </summary>
    public class NinjectEntityIdentityResolver : IEntityIdentityResolver
    {
        private readonly IKernel _kernel;

        public NinjectEntityIdentityResolver(IKernel kernel)
        {
            _kernel = kernel;
        }


        public Maybe<Guid> ResolveUuid<T>(int dbId) where T : class, IHasUuid, IHasId
        {
            return _kernel
                .Get<IGenericRepository<T>>()
                .AsQueryable()
                .ById(dbId)
                .FromNullable()
                .Select(x => x.Uuid);
        }

        public Maybe<int> ResolveDbId<T>(Guid uuid) where T : class, IHasUuid, IHasId
        {
            return _kernel
                .Get<IGenericRepository<T>>()
                .AsQueryable()
                .ByUuid(uuid)
                .FromNullable()
                .Select(x => x.Id);
        }
    }
}
