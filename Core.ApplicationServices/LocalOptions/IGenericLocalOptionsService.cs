using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.LocalOptions;
using Core.DomainModel;

namespace Core.ApplicationServices.LocalOptions
{
    public interface IGenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType>
        where TLocalOptionType : LocalOptionEntity<TOptionType>, new()
        where TOptionType : OptionEntity<TReferenceType>
    {
        Result<IEnumerable<TOptionType>, OperationError> GetByOrganizationUuid(Guid organizationUuid);
        Result<TOptionType, OperationError> GetByOrganizationUuidAndOptionId(Guid organizationUuid, int optionId);

        Result<TOptionType, OperationError> CreateLocalOption(Guid organizationUuid,
            LocalOptionCreateParameters parameters);

        Result<TOptionType, OperationError> PatchLocalOption(Guid organizationUuid, int optionId,
            LocalOptionUpdateParameters parameters);

        Result<TLocalOptionType, OperationError> DeleteLocalOption(Guid organizationUuid, int optionId);
    }
}
