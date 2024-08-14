using Core.Abstractions.Types;
using Core.DomainModel.ItContract;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Contract.ReadModels
{
    public interface IGridLocalItContractRolesService
    {
        Result<IEnumerable<ItContractRole>, OperationError> GetOverviewRoles(Guid organizationUuid)
    }
}
