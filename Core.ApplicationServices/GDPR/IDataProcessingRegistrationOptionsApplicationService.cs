
using System.Collections.Generic;
using Core.ApplicationServices.Model.GDPR;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.GDPR
{
    public interface IDataProcessingRegistrationOptionsApplicationService
    {
        Result<DataProcessingRegistrationOptions, OperationError> GetAssignableDataProcessingRegistrationOptions(int organizationId);

        ISet<int> GetIdsOfAvailableCountryOptions(int organizationId);
        ISet<int> GetIdsOfAvailableDataResponsibleOptions(int organizationId);
    }
}
