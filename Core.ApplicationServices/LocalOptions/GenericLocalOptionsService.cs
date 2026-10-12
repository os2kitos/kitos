using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.LocalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainServices;
using Core.DomainServices.Generic;

namespace Core.ApplicationServices.LocalOptions
{
    public class GenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType> : BaseGenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType>, IGenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType>
    where TLocalOptionType : LocalOptionEntity<TOptionType>, new()
    where TOptionType : OptionEntity<TReferenceType>
    {
        public GenericLocalOptionsService(IGenericRepository<TOptionType> optionsRepository,
            IGenericRepository<TLocalOptionType> localOptionRepository,
            IAuthorizationContext authorizationContext,
            IEntityIdentityResolver identityResolver,
            IDomainEvents domainEvents) : base(optionsRepository, localOptionRepository, authorizationContext, identityResolver, domainEvents)
        {
        }

        public IEnumerable<TOptionType> GetLocalOptions(Guid organizationUuid)
        {
            return BaseGetLocalOptions(organizationUuid, UpdateLocalRoleOptionValues);
        }

        private static void UpdateLocalRoleOptionValues(TOptionType option, TLocalOptionType localOption)
        {
            option.UpdateLocalOptionValues(localOption);
        }


        public Result<TOptionType, OperationError> GetLocalOption(Guid organizationUuid, Guid globalOptionUuid)
        {
            return BaseGetLocalOption(organizationUuid, globalOptionUuid, UpdateLocalRoleOptionValues);
        }

        public Result<TOptionType, OperationError> CreateLocalOption(Guid organizationUuid, LocalOptionCreateParameters parameters)
        {
            return BaseCreateLocalOption(organizationUuid, parameters, UpdateLocalRoleOptionValues);
        }

        public Result<TOptionType, OperationError> PatchLocalOption(Guid organizationUuid, Guid globalOptionUuid, LocalOptionUpdateParameters parameters)
        {
            return BasePatchLocalOption(organizationUuid, globalOptionUuid, parameters, UpdateLocalRoleOptionValues);
        }

        public Result<TOptionType, OperationError> DeleteLocalOption(Guid organizationUuid, Guid globalOptionUuid)
        {
            return BaseDeleteLocalOption(organizationUuid, globalOptionUuid, UpdateLocalRoleOptionValues);
        }
    }
}
