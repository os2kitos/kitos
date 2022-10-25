using Core.Abstractions.Types;

namespace Core.DomainModel.Organization.Strategies
{
    public interface IExternalOrganizationalHierarchyUpdateStrategy
    {
        OrganizationTreeUpdateConsequences ComputeUpdate(ExternalOrganizationUnit root, Maybe<int> levelsIncluded);
        OrganizationTreeUpdateConsequences PerformUpdate(ExternalOrganizationUnit root, Maybe<int> levelsIncluded);
    }
}
