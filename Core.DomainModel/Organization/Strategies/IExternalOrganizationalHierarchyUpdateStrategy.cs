namespace Core.DomainModel.Organization.Strategies
{
    public interface IExternalOrganizationalHierarchyUpdateStrategy
    {
        OrganizationTreeUpdateConsequences ComputeUpdate(ExternalOrganizationUnit root);
        OrganizationTreeUpdateConsequences PerformUpdate(ExternalOrganizationUnit root);
    }
}
