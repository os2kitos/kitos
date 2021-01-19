using System.Collections.Generic;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.GDPR;
using Core.DomainModel.Result;
using Infrastructure.Services.Types;

namespace Core.DomainServices.GDPR
{
    public interface IDataProcessingRegistrationRoleAssignmentsService
    {
        IEnumerable<DataProcessingRegistrationRole> GetApplicableRoles(DataProcessingRegistration registration);
        Result<IQueryable<User>, OperationError> GetUsersWhichCanBeAssignedToRole(DataProcessingRegistration registration, int roleId, Maybe<string> nameEmailQuery);
        Result<DataProcessingRegistrationRight, OperationError> AssignRole(DataProcessingRegistration registration, int roleId, int userId);
        Result<DataProcessingRegistrationRight, OperationError> RemoveRole(DataProcessingRegistration registration, int roleId, int userId);
    }
}
