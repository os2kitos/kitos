using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Core.DomainServices.Queries;
using System;
using System.Linq;

namespace Core.ApplicationServices.GDPR
{
    public interface IDataProcessingRegistrationUuidExtensionService
    {
        Result<IQueryable<DataProcessingRegistration>, OperationError> GetDataProcessingRegistrationsByOrganization(Guid orgUuid, params IDomainQuery<DataProcessingRegistration>[] conditions);
    }
}