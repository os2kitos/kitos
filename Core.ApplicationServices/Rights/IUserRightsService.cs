using System.Collections.Generic;
using Core.Abstractions.Types;
using Core.ApplicationServices.Model.RightsHolder;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Rights
{
    public interface IUserRightsService
    {
        Result<IEnumerable<UserRoleAssociationDTO>, OperationError> GetUsersWithRoleAssignment(OrganizationRole role);
    }
}
