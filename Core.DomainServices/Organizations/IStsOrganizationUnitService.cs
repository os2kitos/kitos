using Core.Abstractions.Types;
using Core.DomainModel.Organization;
using Core.DomainServices.Model.StsOrganization;

namespace Core.DomainServices.Organizations
{
    public interface IStsOrganizationUnitService
    {
        Result<ExternalOrganizationUnit, DetailedOperationError<ResolveOrganizationTreeError>> ResolveOrganizationTree(Organization organization);
    }
}
