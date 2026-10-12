using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.LocalOptions;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.LocalOptions;
using Core.DomainServices;
using Core.DomainServices.Generic;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.LocalOptions
{
    public class GenericLocalRoleOptionsService<TLocalOptionType, TReferenceType, TOptionType> : BaseGenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType>, IGenericLocalRoleOptionsService<TLocalOptionType, TReferenceType, TOptionType>
        where TLocalOptionType : LocalRoleOptionEntity<TOptionType>, new()
        where TOptionType : OptionEntity<TReferenceType>
    {
        public GenericLocalRoleOptionsService(IGenericRepository<TOptionType> optionsRepository,
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

        public Result<TOptionType, OperationError> GetLocalOption(Guid organizationUuid, Guid globalOptionUuid)
        {
            return BaseGetLocalOption(organizationUuid, globalOptionUuid, UpdateLocalRoleOptionValues);
        }

        private static void UpdateLocalRoleOptionValues(TOptionType option, TLocalOptionType localOption)
        {
            option.UpdateLocalRoleOptionValues(localOption);
        }

        public Result<TOptionType, OperationError> CreateLocalOption(Guid organizationUuid, LocalOptionCreateParameters parameters)
        {
            return BaseCreateLocalOption(organizationUuid, parameters, UpdateLocalRoleOptionValues);
        }

        public Result<TOptionType, OperationError> PatchLocalOption(Guid organizationUuid, Guid globalOptionUuid, LocalRoleOptionUpdateParameters parameters)
        {
            return BasePatchLocalOption(organizationUuid, globalOptionUuid, parameters, UpdateLocalRoleOptionValues, UpdateExternallyUsedParameters);
        }

        private static Maybe<OperationError> UpdateExternallyUsedParameters(TLocalOptionType localOption,
            LocalRoleOptionUpdateParameters parameters)
        {
            if (parameters.IsExternallyUsed.HasChange) localOption.UpdateIsExternallyUsed(parameters.IsExternallyUsed.NewValue);
            if (parameters.ExternallyUsedDescription.HasChange)
            {
                var externallyUsedDescriptionError = localOption.UpdateExternallyUsedDescription(parameters.ExternallyUsedDescription.NewValue);
                if (externallyUsedDescriptionError.HasValue)
                    return externallyUsedDescriptionError.Value;
            }
            return Maybe<OperationError>.None;
        }

        public Result<TOptionType, OperationError> DeleteLocalOption(Guid organizationUuid, Guid globalOptionUuid)
        {
            return BaseDeleteLocalOption(organizationUuid, globalOptionUuid, UpdateLocalRoleOptionValues);
        }
    }
}
