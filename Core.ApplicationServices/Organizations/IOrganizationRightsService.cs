using Core.ApplicationServices.Model.Result;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationRightsService
    {
        Result<OrganizationRight, OperationFailure> AddRightToUser(int organizationId, OrganizationRight right);
        Result<OrganizationRight, OperationFailure> RemoveRole(int organizationId, int userId, OrganizationRole rightId);
        Result<OrganizationRight, OperationFailure> RemoveRole(int rightId);
    }
}
