using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;

namespace Core.DomainServices.Organizations
{
    public interface IStsOrganizationUnitService
    {
        //TODO: Detailed error
        Result<StsOrganizationUnit, OperationError> ResolveOrganizationTree(Organization organization);
    }
}
