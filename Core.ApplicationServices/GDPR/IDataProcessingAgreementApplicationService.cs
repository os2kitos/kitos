using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.ItSystem;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.ApplicationServices.GDPR
{
    public interface IDataProcessingAgreementApplicationService
    {
        Result<DataProcessingAgreement, OperationError> Create(int organizationId, string name);
        Maybe<OperationError> ValidateSuggestedNewAgreement(int organizationId, string name);
        Result<DataProcessingAgreement, OperationError> Delete(int id);
        Result<DataProcessingAgreement, OperationError> Get(int id);
        Result<IQueryable<DataProcessingAgreement>, OperationError> GetOrganizationData(int organizationId, int skip, int take);
        Result<DataProcessingAgreement, OperationError> UpdateName(int id, string name);
        Result<(DataProcessingAgreement agreement, IEnumerable<DataProcessingAgreementRole> roles), OperationError> GetAvailableRoles(int id);
        Result<IEnumerable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(int id, int roleId, string nameEmailQuery, int pageSize);
        Result<DataProcessingAgreementRight, OperationError> AssignRole(int id, int roleId, int userId);
        Result<DataProcessingAgreementRight, OperationError> RemoveRole(int id, int roleId, int userId);
        Result<IEnumerable<ItSystem>, OperationError> GetSystemsWhichCanBeAssigned(int id, string nameQuery, int pageSize);
        Result<ItSystem, OperationError> AssignSystem(int id, int systemId);
        Result<ItSystem, OperationError> RemoveSystem(int id, int systemId);
    }
}
