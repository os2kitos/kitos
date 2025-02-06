using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Types;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Model.Contracts;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItContract;
using Core.DomainModel.Organization;
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

        Result<IEnumerable<(int year, int quarter)>, OperationError> GetAppliedProcurementPlansByUuid(
            Guid organizationUuid);
        Maybe<OperationError> SetResponsibleUnit(int contractId, Guid targetUnitUuid);
        Maybe<OperationError> RemoveResponsibleUnit(int contractId);
        Maybe<OperationError> RemovePaymentResponsibleUnits(int contractId, bool isInternal, IEnumerable<int> paymentIds);
        Maybe<OperationError> TransferPayments(int contractId, Guid targetUnitUuid, bool isInternal, IEnumerable<int> paymentIds);
        Result<ContractPermissions, OperationError> GetPermissions(Guid uuid);
        Result<ResourceCollectionPermissionsResult, OperationError> GetCollectionPermissions(Guid organizationUuid);
    }
}
