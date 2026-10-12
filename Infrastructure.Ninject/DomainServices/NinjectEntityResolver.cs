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
    public class NinjectEntityResolver : IEntityResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public NinjectEntityResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Result<T, OperationError> ResolveEntityFromUuid<T>(Guid uuid) where T : class, IHasUuid
        {
            return _serviceProvider
                .GetRequiredService<IGenericRepository<T>>()
                .AsQueryable()
                .ByUuid(uuid)
                .FromNullable()
                .Match<Result<T, OperationError>>(t => t, () => new OperationError($"Failed to resolve {typeof(T).Name} with Uuid: {uuid}", OperationFailure.BadInput));
        }
    }
}
