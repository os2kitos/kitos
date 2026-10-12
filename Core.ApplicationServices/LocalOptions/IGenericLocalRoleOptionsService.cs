using Core.Abstractions.Types;
using Core.ApplicationServices.Model.LocalOptions;
using Core.DomainModel;
using Core.DomainModel.LocalOptions;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.LocalOptions
{
    public interface IGenericLocalRoleOptionsService<TLocalOptionType, TReferenceType, TOptionType>
        where TLocalOptionType : LocalRoleOptionEntity<TOptionType>, new()
        where TOptionType : OptionEntity<TReferenceType>
    {
        IEnumerable<TOptionType> GetLocalOptions(Guid organizationUuid);
        Result<TOptionType, OperationError> GetLocalOption(Guid organizationUuid, Guid globalOptionUuid);
        Result<TOptionType, OperationError> CreateLocalOption(Guid organizationUuid,
            LocalOptionCreateParameters parameters);

        Result<TOptionType, OperationError> PatchLocalOption(Guid organizationUuid, Guid globalOptionUuid,
            LocalRoleOptionUpdateParameters parameters);

        Result<TOptionType, OperationError> DeleteLocalOption(Guid organizationUuid, Guid globalOptionUuid);
    }
}
