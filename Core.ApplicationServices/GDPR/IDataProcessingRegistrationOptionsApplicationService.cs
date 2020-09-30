
using System;
using System.Collections.Generic;
using Core.ApplicationServices.Model.GDPR;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.GDPR
{
    public interface IDataProcessingRegistrationOptionsApplicationService
    {
        Result<DataProcessingRegistrationOptions, OperationError> GetAssignableDataProcessingRegistrationOptions(int organizationId);
        [Obsolete("Replace by extending the registration options (from that the controller may create the 'id maps')")]
        ISet<int> GetIdsOfAvailableCountryOptions(int organizationId);
        [Obsolete("Replace by extending the registration options (from that the controller may create the 'id maps')")]
        ISet<int> GetIdsOfAvailableDataResponsibleOptions(int organizationId);
    }
}
