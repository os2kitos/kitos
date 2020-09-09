using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingAgreementRoleAssignmentsService
    {
        IEnumerable<DataProcessingAgreementRole> GetRolesWhichCanBeAssignedToAgreement(DataProcessingAgreement agreement);
        Result<IQueryable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(DataProcessingAgreement agreement, int roleId, Maybe<string> nameEmailQuery);
        Result<DataProcessingAgreementRight, OperationError> AssignRole(DataProcessingAgreement agreement, int roleId, int userId);
        Result<DataProcessingAgreementRight, OperationError> RemoveRole(DataProcessingAgreement agreement, int roleId, int userId);
    }
}
