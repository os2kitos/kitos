using System;
using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainModel;
using Core.DomainModel.ItSystemUsage;

namespace Core.DomainServices.SystemUsage
{
    public interface IAttachedOptionsAssignmentService<TOption, TTarget> where TOption : OptionEntity<TTarget>
    {
        Result<IEnumerable<TOption>, OperationError> UpdateAssignedOptions(ItSystemUsage systemUsage, IEnumerable<Guid> optionUuids);
    }
}
