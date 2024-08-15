using Core.Abstractions.Types;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.Contract.ReadModels
{
    public interface IGridLocalItContractRolesService
    {
        Result<IEnumerable<(string Name, Guid Uuid, int Id)>, OperationError> GetOverviewRoles(Guid organizationUuid);
    }
}
