﻿using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Contract
{
    public interface IEconomyStreamService
    {
        Maybe<OperationError> Delete(int id);
        Maybe<OperationError> TransferRange(OrganizationUnit targetUnit, IEnumerable<int> ids);
        Maybe<OperationError> DeleteRange(IEnumerable<int> ids);
        Maybe<OperationError> DeleteRange(IEnumerable<EconomyStream> entities);
    }
}
