using System;
using Core.Abstractions.Caching;
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
    /// Ninject adapter for the <see cref="IEntityIdentityResolver"/> domain service.
    /// This implementation ensures that we can quickly exchange id for uuid without needing a reference to all repositories in the client context.
    /// </summary>
    public class NinjectEntityIdentityResolver : IEntityIdentityResolver
    {
        public class SingleValue<T>
        {
            public T Value { get; set; }
        }
        private readonly IKernel _kernel;
        private readonly IObjectCache _objectCache;

        public NinjectEntityIdentityResolver(IKernel kernel, IObjectCache objectCache)
        {
            _kernel = kernel;
            _objectCache = objectCache;
        }

        private string GetCacheKey<T>(int dbId)
        {
            return $"DBID({dbId})->UUID:{typeof(T).FullName}";
        }

        private string GetCacheKey<T>(Guid uuid)
        {
            return $"UUID({uuid:N})->DBID:{typeof(T).FullName}";
        }

        public Maybe<Guid> ResolveUuid<T>(int dbId) where T : class, IHasUuid, IHasId
        {
            var cacheKey = GetCacheKey<T>(dbId);
            return _objectCache
                .Read<SingleValue<Guid>>(cacheKey)
                .Match
                (
                    existing => existing.Value,
                    () => LoadUuidFromDb<T>(dbId, cacheKey)
                );
        }

        private Maybe<Guid> LoadUuidFromDb<T>(int dbId, string cacheKey) where T : class, IHasUuid, IHasId
        {
            return _kernel
                .Get<IGenericRepository<T>>()
                .AsQueryable()
                .ById(dbId)
                .FromNullable()
                .Select(x =>
                {
                    var uuid = x.Uuid;
                    _objectCache.Write(new SingleValue<Guid> { Value = uuid }, cacheKey, TimeSpan.FromHours(1));
                    return uuid;
                });
        }

        public Maybe<int> ResolveDbId<T>(Guid uuid) where T : class, IHasUuid, IHasId
        {
            var cacheKey = GetCacheKey<T>(uuid);
            return _objectCache
                .Read<SingleValue<int>>(cacheKey)
                .Match
                (
                    existing => existing.Value,
                    () => LoadDbIdFromDb<T>(uuid, cacheKey)
                );
        }

        private Maybe<int> LoadDbIdFromDb<T>(Guid uuid, string cacheKey) where T : class, IHasUuid, IHasId
        {
            return _kernel
                .Get<IGenericRepository<T>>()
                .AsQueryable()
                .ByUuid(uuid)
                .FromNullable()
                .Select(x =>
                {
                    var dbId = x.Id;
                    _objectCache.Write(new SingleValue<int>() { Value = dbId }, cacheKey, TimeSpan.FromHours(1));
                    return dbId;
                });
        }
    }
}
