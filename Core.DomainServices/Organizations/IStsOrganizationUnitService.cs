using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;

namespace Core.DomainServices.Organizations
{
    public interface IStsOrganizationUnitService
    {
        Result<StsOrganizationUnit, DetailedOperationError<ResolveOrganizationTreeError>> ResolveOrganizationTree(Organization organization);
    }
}
