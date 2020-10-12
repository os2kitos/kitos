using System.Collections.Generic;
using System.Linq;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.Result;

namespace Core.ApplicationServices.Contract
{
    public interface IItContractService
    {
        IQueryable<ItContract> GetAllByOrganization(int orgId, string optionalNameSearch = null);
        Result<ItContract, OperationFailure> Delete(int id);

        Result<DataProcessingRegistration, OperationError> AssignDataProcessingRegistration(int id, int dataProcessingRegistrationId);

        Result<DataProcessingRegistration, OperationError> RemoveDataProcessingRegistration(int id, int dataProcessingRegistrationId);
        Result<IEnumerable<DataProcessingRegistration>, OperationError> GetDataProcessingRegistrationsWhichCanBeAssigned(int id, string nameQuery, int pageSize);
    }
}
