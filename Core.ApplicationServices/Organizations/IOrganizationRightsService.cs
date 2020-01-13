using Core.ApplicationServices.Model.Result;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationRightsService
    {
        TwoTrackResult<OrganizationRight, OperationFailure> RemoveRole(int organizationId, int userId, OrganizationRole rightId);
        TwoTrackResult<OrganizationRight, OperationFailure> RemoveRole(int rightId);
    }
}
