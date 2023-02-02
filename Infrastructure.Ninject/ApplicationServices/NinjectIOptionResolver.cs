using System;
using Core.Abstractions.Types;
using Core.ApplicationServices.OptionTypes;
using Core.DomainModel;
using Ninject;

namespace Infrastructure.Ninject.ApplicationServices
{
    public class NinjectIOptionResolver : IOptionResolver
    {
        private readonly IKernel _kernel;

        public NinjectIOptionResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        public Result<(TOption option, bool available), OperationError> GetOptionType<TReference, TOption>(Guid organizationUuid, Guid optionUuid) where TOption : OptionEntity<TReference>
        {
            var applicationService = _kernel.Get<IOptionsApplicationService<TReference, TOption>>();
            return applicationService
                .GetOptionType(organizationUuid, optionUuid)
                .Select(option => (option.option.Option, option.available));
        }
    }
}
