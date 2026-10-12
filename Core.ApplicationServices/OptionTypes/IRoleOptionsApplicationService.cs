using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainServices.Model.Options;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.OptionTypes
{
    public interface IRoleOptionsApplicationService<TReference, TOption> where TOption : OptionEntity<TReference>
    {
        Result<IEnumerable<OptionDescriptor<TOption>>, OperationError> GetOptionTypes(Guid organizationUuid);
    }
}
