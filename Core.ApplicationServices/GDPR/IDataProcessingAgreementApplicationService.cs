using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
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
        Result<IEnumerable<DataProcessingAgreementRole>, OperationError> GetAvailableRoles(int id);
        Result<IEnumerable<User>, OperationError> GetAvailableUsers(int id, int roleId, string nameEmailQuery, int pageSize);
        Result<DataProcessingAgreementRight, OperationError> AssignRole(int id, int roleId, int userId);
        Result<DataProcessingAgreementRight, OperationError> RemoveRole(int id, int roleId, int userId);
    }
}
