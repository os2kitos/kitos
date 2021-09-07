using Core.DomainModel;
using System;
using System.Collections.Generic;
using Core.Abstractions.Types;

namespace Core.ApplicationServices.OptionTypes
{
    public interface IOptionsApplicationService<TReference, TOption> where TOption : OptionEntity<TReference>
    {
        Result<IEnumerable<TOption>, OperationError> GetOptionTypes(Guid organizationUuid);
        Result<(TOption option, bool available), OperationError> GetOptionType(Guid organizationUuid, Guid businessTypeUuid);
    }
}
