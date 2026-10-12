using System;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices;
using Core.DomainServices.Extensions;
using Core.DomainServices.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Ninject.DomainServices
{
    public class NinjectEntityIdentityResolver : IEntityIdentityResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public NinjectEntityIdentityResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Maybe<Guid> ResolveUuid<T>(int dbId) where T : class, IHasUuid, IHasId
        {
            return _serviceProvider
                .GetRequiredService<IGenericRepository<T>>()
                .AsQueryable()
                .ById(dbId)
                .FromNullable()
                .Select(x => x!.Uuid);
        }

        public Maybe<int> ResolveDbId<T>(Guid uuid) where T : class, IHasUuid, IHasId
        {
            return _serviceProvider
                .GetRequiredService<IGenericRepository<T>>()
                .AsQueryable()
                .ByUuid(uuid)
                .FromNullable()
                .Select(x => x!.Id);
        }
    }
}
