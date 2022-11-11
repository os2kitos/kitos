using Core.Abstractions.Types;

namespace Core.DomainModel.Organization.Strategies
{
    public interface IExternalOrganizationalHierarchyUpdateStrategy
    {
        OrganizationTreeUpdateConsequences ComputeUpdate(ExternalOrganizationUnit root);
        Result<OrganizationTreeUpdateConsequences, OperationError> PerformUpdate(ExternalOrganizationUnit root);
    }
}
