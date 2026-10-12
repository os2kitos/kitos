using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Ninject.ApplicationServices
{
    public class NinjectIOptionResolver : IOptionResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public NinjectIOptionResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Result<(TOption option, bool available), OperationError> GetOptionType<TReference, TOption>(Guid organizationUuid, Guid optionUuid) where TOption : OptionEntity<TReference>
        {
            var applicationService = _serviceProvider.GetRequiredService<IOptionsApplicationService<TReference, TOption>>();
            return applicationService
                .GetOptionType(organizationUuid, optionUuid)
                .Select(option => (option.option.Option, option.available));
        }
    }
}
