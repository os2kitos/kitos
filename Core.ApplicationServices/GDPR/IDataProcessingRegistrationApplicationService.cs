using System;
using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Organization;
using Core.DomainModel.Result;
using Core.DomainModel.Shared;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.GDPR
{
    public interface IDataProcessingRegistrationApplicationService
    {
        Result<DataProcessingRegistration, OperationError> Create(int organizationId, string name);
        Maybe<OperationError> ValidateSuggestedNewRegistrationName(int organizationId, string name);
        Result<DataProcessingRegistration, OperationError> Delete(int id);
        Result<DataProcessingRegistration, OperationError> Get(int id);
        Result<IQueryable<DataProcessingRegistration>, OperationError> GetOrganizationData(int organizationId, int skip, int take);
        Result<DataProcessingRegistration, OperationError> UpdateName(int id, string name);
        Result<ExternalReference, OperationError> SetMasterReference(int id, int referenceId);
        Result<(DataProcessingRegistration registration, IEnumerable<DataProcessingRegistrationRole> roles), OperationError> GetAvailableRoles(int id);
        Result<IEnumerable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(int id, int roleId, string nameEmailQuery, int pageSize);
        Result<DataProcessingRegistrationRight, OperationError> AssignRole(int id, int roleId, int userId);
        Result<DataProcessingRegistrationRight, OperationError> RemoveRole(int id, int roleId, int userId);
        Result<IEnumerable<ItSystem>, OperationError> GetSystemsWhichCanBeAssigned(int id, string nameQuery, int pageSize);
        Result<ItSystem, OperationError> AssignSystem(int id, int systemId);
        Result<ItSystem, OperationError> RemoveSystem(int id, int systemId);
        Result<DataProcessingRegistration, OperationError> UpdateOversightInterval(int id, YearMonthIntervalOption oversightInterval);
        Result<DataProcessingRegistration, OperationError> UpdateOversightIntervalRemark(int id, string remark);
        Result<IEnumerable<Organization>, OperationError> GetDataProcessorsWhichCanBeAssigned(int id, string nameQuery, int pageSize);
        Result<Organization, OperationError> AssignDataProcessor(int id, int organizationId);
        Result<Organization, OperationError> RemoveDataProcessor(int id, int organizationId);
        Result<IEnumerable<Organization>, OperationError> GetSubDataProcessorsWhichCanBeAssigned(int id, string nameQuery, int pageSize);
        Result<DataProcessingRegistration, OperationError> SetSubDataProcessorsState(int id, YesNoUndecidedOption state);
        Result<Organization, OperationError> AssignSubDataProcessor(int id, int organizationId);
        Result<Organization, OperationError> RemoveSubDataProcessor(int id, int organizationId);
        Result<DataProcessingRegistration, OperationError> UpdateIsAgreementConcluded(int id, YesNoIrrelevantOption concluded);
        Result<DataProcessingRegistration, OperationError> UpdateAgreementConcludedAt(int id, DateTime? concludedAtDate);
        Result<DataProcessingRegistration, OperationError> UpdateTransferToInsecureThirdCountries(int id, YesNoUndecidedOption transferToInsecureThirdCountries);
        Result<DataProcessingCountryOption, OperationError> AssignInsecureThirdCountry(int id, int countryId);
        Result<DataProcessingCountryOption, OperationError> RemoveInsecureThirdCountry(int id, int countryId);
        Result<DataProcessingBasisForTransferOption, OperationError> AssignBasisForTransfer(int id, int basisForTransferId);
        Result<DataProcessingBasisForTransferOption, OperationError> ClearBasisForTransfer(int id);
        Result<DataProcessingRegistration, OperationError> UpdateIsOversightCompleted(int id, YesNoUndecidedOption completed);
        Result<DataProcessingRegistration, OperationError> UpdateLatestOversightDate(int id, DateTime? latestDate);
        Result<DataProcessingRegistration, OperationError> UpdateIsOversightCompletedRemark(int id, string note);
    }
}
