using Core.ApplicationServices.Model.Result;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.Result;

namespace Core.ApplicationServices.Organizations
{
    public interface IOrganizationRightsService
    {
        TwoTrackResult<OrganizationRight, GenericOperationFailure> RemoveRole(int organizationId, int userId, OrganizationRole rightId);
        TwoTrackResult<OrganizationRight, GenericOperationFailure> RemoveRole(int rightId);
    }
}
