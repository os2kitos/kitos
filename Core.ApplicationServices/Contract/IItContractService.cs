using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.Contracts;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainServices.Queries;


namespace Core.ApplicationServices.Contract
{
    public interface IItContractService
    {
        Result<ItContract, OperationError> Create(int organizationId, string name);
        Result<IQueryable<ItContract>,OperationError> GetAllByOrganization(int orgId, string optionalNameSearch = null);
        Result<ItContract, OperationFailure> Delete(int id);

        Result<DataProcessingRegistration, OperationError> AssignDataProcessingRegistration(int id, int dataProcessingRegistrationId);

        Result<DataProcessingRegistration, OperationError> RemoveDataProcessingRegistration(int id, int dataProcessingRegistrationId);
        Result<IEnumerable<DataProcessingRegistration>, OperationError> GetDataProcessingRegistrationsWhichCanBeAssigned(int id, string nameQuery, int pageSize);
        Result<ItContract, OperationError> GetContract(Guid uuid);
        Result<ItContract, OperationError> GetContract(int id);
        Result<bool,OperationError> CanCreateNewContractWithName(string name, int organizationId);
        Maybe<OperationError> ValidateNewName(int contractId, string name);
        IQueryable<ItContract> Query(params IDomainQuery<ItContract>[] conditions);
        Result<ContractOptions, OperationError> GetAssignableContractOptions(int organizationId);
        Result<IEnumerable<(int year, int quarter)>, OperationError> GetAppliedProcurementPlans(int organizationId);
        Result<ItContract, OperationError> TransferContractResponsibleUnit(int targetUnitId, int contractId);
        Result<ItContract, OperationError> RemoveContractResponsibleUnit(int contractId);
        IEnumerable<ItContract> GetContractsByResponsibleUnitId(int unitId);
    }
}
