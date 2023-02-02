using Core.DomainModel;
using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainServices.Model.Options;

namespace Core.ApplicationServices.OptionTypes
{
    public interface IOptionsApplicationService<TReference, TOption> where TOption : OptionEntity<TReference>
    {
        Result<IEnumerable<OptionDescriptor<TOption>>, OperationError> GetOptionTypes(Guid organizationUuid);
        Result<(OptionDescriptor<TOption> option, bool available), OperationError> GetOptionType(Guid organizationUuid, Guid optionTypeUuid);
    }
}
