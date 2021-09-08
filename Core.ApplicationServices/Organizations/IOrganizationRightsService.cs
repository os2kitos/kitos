using Core.Abstractions.Types;
using Core.DomainModel.Organization;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationRightsService
    {
        Result<OrganizationRight, OperationFailure> AssignRole(int organizationId, int userId, OrganizationRole roleId);
        Result<OrganizationRight, OperationFailure> RemoveRole(int organizationId, int userId, OrganizationRole rightId);
        Result<OrganizationRight, OperationFailure> RemoveRole(int rightId);
    }
}
