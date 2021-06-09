using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using System;
using System.Collections.Generic;

namespace Core.ApplicationServices.LocalOption
{
    public interface IBusinessTypeApplicationService
    {
        Result<IEnumerable<BusinessType>, OperationError> GetBusinessTypes(Guid organizationUuid);
        Result<(BusinessType option, bool available), OperationError> GetBusinessType(Guid organizationUuid, Guid businessTypeUuid);
    }
}
